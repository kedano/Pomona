#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright � 2015 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

using Pomona.Common.Internals;

namespace Pomona.CodeGen
{
    public delegate void GeneratePropertyMethods(PropertyDefinition targetProperty,
                                                 PropertyDefinition proxyProperty,
                                                 TypeReference proxyBaseType,
                                                 TypeReference proxyTargetType);

    public class ProxyBuilder
    {
        private readonly bool isPublic;
        private readonly ModuleDefinition module;
        private readonly GeneratePropertyMethods onGeneratePropertyMethods;
        private readonly string proxyNamespace;
        private readonly TypeReference proxySuperBaseTypeDef;
        private string proxyNameFormat;


        public ProxyBuilder(ModuleDefinition module,
                            string proxyNameFormat,
                            TypeReference proxySuperBaseTypeDef,
                            bool isPublic,
                            GeneratePropertyMethods onGeneratePropertyMethods = null,
                            string proxyNamespace = null)
        {
            this.proxyNamespace = proxyNamespace ?? module.Assembly.Name.Name;
            this.module = module;
            this.proxySuperBaseTypeDef = proxySuperBaseTypeDef;
            this.onGeneratePropertyMethods = onGeneratePropertyMethods;
            this.proxyNameFormat = proxyNameFormat;
            this.isPublic = isPublic;
        }


        public virtual ModuleDefinition Module
        {
            get { return this.module; }
        }

        public virtual string ProxyNameFormat
        {
            get { return this.proxyNameFormat; }
            protected set { this.proxyNameFormat = value; }
        }


        public TypeDefinition CreateProxyType(string nameBase,
                                              IEnumerable<TypeDefinition> interfacesToImplement,
                                              TypeDefinition proxyBase)
        {
            proxyBase = proxyBase ?? this.proxySuperBaseTypeDef.Resolve();
            MethodReference proxyBaseCtor = proxyBase.GetConstructors().First(x => x.Parameters.Count == 0);
            proxyBaseCtor = Module.Import(proxyBaseCtor);

            var proxyTypeName = string.Format(this.proxyNameFormat, nameBase);

            var typeAttributes = this.isPublic
                ? TypeAttributes.Public
                : TypeAttributes.NotPublic;

            var proxyType = new TypeDefinition(this.proxyNamespace,
                                               proxyTypeName,
                                               typeAttributes,
                                               this.module.Import(proxyBase));
            Module.Types.Add(proxyType);

            foreach (var interfaceDef in interfacesToImplement)
                proxyType.Interfaces.Add(this.module.Import(interfaceDef));

            // Empty public constructor
            var ctor = new MethodDefinition(
                ".ctor",
                MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName
                | MethodAttributes.Public,
                Module.TypeSystem.Void);

            ctor.Body.MaxStackSize = 8;
            var ctorIlProcessor = ctor.Body.GetILProcessor();
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Call, proxyBaseCtor));
            ctorIlProcessor.Append(Instruction.Create(OpCodes.Ret));

            proxyType.Methods.Add(ctor);

            var interfaces =
                interfacesToImplement.SelectMany(GetAllInterfacesRecursive)
                                     .Except(proxyBase.Interfaces.SelectMany(x => GetAllInterfacesRecursive(x.Resolve())))
                                     .Distinct()
                                     .ToList();

            var propertiesToCreate = interfaces.SelectMany(x => x.Properties).ToList();
            foreach (var targetProp in propertiesToCreate)
            {
                var proxyPropDef = AddProperty(proxyType, targetProp.Name, this.module.Import(targetProp.PropertyType));
                OnGeneratePropertyMethods(
                    targetProp,
                    proxyPropDef,
                    this.proxySuperBaseTypeDef,
                    this.module.Import(targetProp.DeclaringType),
                    interfacesToImplement.First());
            }

            return proxyType;
        }


        protected virtual void OnGeneratePropertyMethods(PropertyDefinition targetProp,
                                                         PropertyDefinition proxyProp,
                                                         TypeReference proxyBaseType,
                                                         TypeReference proxyTargetType,
                                                         TypeReference rootProxyTargetType)
        {
            if (this.onGeneratePropertyMethods == null)
            {
                throw new InvalidOperationException(
                    "Either onGenerateMethodsFunc must be provided in argument, or OnGenerateMethods must be overrided");
            }

            this.onGeneratePropertyMethods(targetProp, proxyProp, proxyBaseType, proxyTargetType);
        }


        /// <summary>
        /// Create property with public getter and setter, with no method defined.
        /// </summary>
        private PropertyDefinition AddProperty(TypeDefinition declaringType, string name, TypeReference propertyType)
        {
            if (declaringType.Properties.Any(x => x.Name == name))
                throw new InvalidOperationException("Duplicate method is not good...");

            var proxyPropDef = new PropertyDefinition(name, PropertyAttributes.None, propertyType);
            var proxyPropGetter = new MethodDefinition(
                "get_" + name,
                MethodAttributes.NewSlot | MethodAttributes.SpecialName |
                MethodAttributes.HideBySig
                | MethodAttributes.Virtual | MethodAttributes.Public,
                propertyType);
            proxyPropDef.GetMethod = proxyPropGetter;

            var proxyPropSetter = new MethodDefinition(
                "set_" + name,
                MethodAttributes.NewSlot | MethodAttributes.SpecialName |
                MethodAttributes.HideBySig
                | MethodAttributes.Virtual | MethodAttributes.Public,
                Module.TypeSystem.Void);

            proxyPropSetter.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, propertyType));
            proxyPropDef.SetMethod = proxyPropSetter;

            declaringType.Methods.Add(proxyPropGetter);
            declaringType.Methods.Add(proxyPropSetter);
            declaringType.Properties.Add(proxyPropDef);
            return proxyPropDef;
        }


        private static IEnumerable<TypeDefinition> GetAllInterfacesRecursive(TypeDefinition typeDefinition)
        {
            return
                typeDefinition.WrapAsEnumerable().Concat(
                    typeDefinition.Interfaces.SelectMany(x => GetAllInterfacesRecursive(x.Resolve())));
        }
    }
}