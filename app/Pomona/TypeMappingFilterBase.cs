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
using System.Linq.Expressions;
using System.Reflection;

using Nancy.Extensions;

using Newtonsoft.Json;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.FluentMapping;

namespace Pomona
{
    public abstract class TypeMappingFilterBase : ITypeMappingFilter, IWrappableTypeMappingFilter
    {
        private static readonly HashSet<Type> jsonSupportedNativeTypes;
        private readonly HashSet<Type> sourceTypesCached;


        static TypeMappingFilterBase()
        {
            jsonSupportedNativeTypes = new HashSet<Type>(TypeUtils.GetNativeTypes());
        }


        protected TypeMappingFilterBase(IEnumerable<Type> sourceTypes)
        {
            this.sourceTypesCached = new HashSet<Type>(sourceTypes);
            BaseFilter = this;
        }


        private HashSet<Type> SourceTypes
        {
            get { return this.sourceTypesCached; }
        }


        private bool IsNativelySupportedType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return jsonSupportedNativeTypes.Contains(type) || IsNullableAllowedNativeType(type);
        }


        private bool IsNullableAllowedNativeType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return IsNullableType(type) &&
                   TypeIsMapped(type.GetGenericArguments()[0]);
        }


        private static bool IsNullableType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }


        public virtual string ApiVersion
        {
            get { return "0.1.0"; }
        }

        public ITypeMappingFilter BaseFilter { get; set; }

        public virtual ClientMetadata ClientMetadata
        {
            get { return new ClientMetadata(informationalVersion : ApiVersion); }
        }


        public virtual bool GenerateIndependentClient()
        {
            return true;
        }


        public virtual DefaultPropertyInclusionMode GetDefaultPropertyInclusionMode()
        {
            return DefaultPropertyInclusionMode.AllPropertiesAreIncludedByDefault;
        }

        #region ITypeMappingFilter Members

        public virtual bool ClientEnumIsGeneratedAsStringEnum(Type enumType)
        {
            return false;
        }


        public virtual bool ClientPropertyIsExposedAsRepository(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            return false;
        }


        public virtual bool DeleteOfTypeIsAllowed(Type type)
        {
            return false;
        }


        public virtual IEnumerable<PropertyInfo> GetAllPropertiesOfType(Type type, BindingFlags bindingFlags)
        {
            return type.GetProperties(bindingFlags);
        }


        public virtual PropertyInfo GetChildToParentProperty(Type type)
        {
            return null;
        }


        public virtual IEnumerable<CustomAttributeData> GetClientLibraryAttributes(MemberInfo member)
        {
            return member.GetCustomAttributesData().Where(x => x.Constructor.DeclaringType == typeof(ObsoleteAttribute));
        }


        public virtual Type GetClientLibraryType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return null;
        }


        public virtual JsonConverter GetJsonConverterForType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            //if (type == typeof(byte[]))
            //    return new BinaryConverter();
            return null;
        }


        public virtual Action<object> GetOnDeserializedHook(Type type)
        {
            return null;
        }


        public virtual PropertyInfo GetParentToChildProperty(Type type)
        {
            return null;
        }


        public virtual string GetPluralNameForType(Type type)
        {
            return SingularToPluralTranslator.CamelCaseToPlural(BaseFilter.GetTypeMappedName(type));
        }


        public virtual Type GetPostReturnType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return type;
        }


        public virtual HttpMethod GetPropertyAccessMode(PropertyInfo propertyInfo, ConstructorSpec constructorSpec)
        {
            var mode = (propertyInfo.CanRead ? HttpMethod.Get : 0);
            if ((propertyInfo.CanWrite && propertyInfo.GetSetMethod() != null))
                mode |= HttpMethod.Put | HttpMethod.Post;
            if (constructorSpec != null)
            {
                var paramSpec = constructorSpec.GetParameterSpec(propertyInfo);
                if (paramSpec != null)
                    mode |= HttpMethod.Post;
            }
            return mode;
        }


        public virtual PropertyCreateMode GetPropertyCreateMode(Type type,
                                                                PropertyInfo propertyInfo,
                                                                ParameterInfo ctorParameterInfo)
        {
            if (ctorParameterInfo != null)
                return PropertyCreateMode.Required;

            if (propertyInfo.PropertyType.IsCollection() ||
                (propertyInfo.CanWrite && propertyInfo.GetSetMethod() != null))
                return PropertyCreateMode.Optional;
            return PropertyCreateMode.Excluded;
        }


        public virtual ExpandMode GetPropertyExpandMode(Type type, PropertyInfo propertyInfo)
        {
            return ExpandMode.Default;
        }


        public virtual PropertyFlags? GetPropertyFlags(PropertyInfo propertyInfo)
        {
            return (propertyInfo.CanRead ? PropertyFlags.IsReadable | PropertyFlags.AllowsFiltering : 0) |
                   (propertyInfo.CanWrite ? PropertyFlags.IsWritable : 0);
        }


        public virtual LambdaExpression GetPropertyFormula(Type type, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            return null;
        }


        public virtual Func<object, IContainer, object> GetPropertyGetter(Type type, PropertyInfo propertyInfo)
        {
            return null;
        }


        public virtual HttpMethod GetPropertyItemAccessMode(Type type, PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.IsGenericInstanceOf(typeof(IDictionary<,>)))
                return HttpMethod.Delete | HttpMethod.Put | HttpMethod.Get | HttpMethod.Post | HttpMethod.Patch;
            if (propertyInfo.PropertyType.IsCollection())
                return HttpMethod.Delete | HttpMethod.Get | HttpMethod.Patch | HttpMethod.Post;
            return HttpMethod.Get;
        }


        public virtual string GetPropertyMappedName(Type type, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            return propertyInfo.Name;
        }


        public virtual Action<object, object, IContainer> GetPropertySetter(Type type, PropertyInfo propertyInfo)
        {
            return null;
        }


        public virtual Type GetPropertyType(Type type, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            return propertyInfo.PropertyType;
        }


        public virtual IEnumerable<Type> GetResourceHandlers(Type type)
        {
            return null;
        }


        public virtual ConstructorSpec GetTypeConstructor(Type type)
        {
            return
                type.GetConstructors().OrderByDescending(x => x.GetParameters().Length).Select(
                    x => ConstructorSpec.FromConstructorInfo(x, defaultFactory : () => null)).FirstOrDefault(
                        x => x != null);
        }


        public virtual bool GetTypeIsAbstract(Type type)
        {
            return type.IsAbstract;
        }


        public virtual Type GetUriBaseType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (TypeIsMappedAsValueObject(type))
                return null;

            return type;
        }


        public virtual string GetUrlRelativePath(Type type)
        {
            return
                NameUtils.ConvertCamelCaseToUri(BaseFilter.GetPluralNameForType(BaseFilter.GetUriBaseType(type) ?? type));
        }


        public virtual bool IsIndependentTypeRoot(Type type)
        {
            return false;
        }


        public virtual bool PatchOfTypeIsAllowed(Type type)
        {
            return true;
        }


        public virtual bool PostOfTypeIsAllowed(Type type)
        {
            return true;
        }


        public virtual bool PropertyIsAttributes(Type type, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            return false;
        }


        public virtual bool PropertyIsEtag(Type type, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            return false;
        }


        public virtual bool PropertyIsIncluded(Type type, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            var getMethod = propertyInfo.GetGetMethod(true);
            return getMethod != null && getMethod.IsPublic && !getMethod.IsStatic;
        }


        public virtual bool PropertyIsPrimaryId(Type type, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            return propertyInfo.Name.ToLower() == "id";
        }


        public virtual Type ResolveRealTypeForProxy(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            // TODO: Implement some crude heuristics to check whether a type is a proxy type,
            //       that should be treated as its base type.
            //
            //       Maybe look for whether type is public (will proxys normally be internal?), or
            //       that it resides in different assembly than base type.
            //
            //       Or maybe just see whether type is mapped.
            //

            // Lets just try this for now:
            if (SourceTypes.Contains(type))
                return type;

            if (type.BaseType != null && SourceTypes.Contains(type.BaseType))
                return type.BaseType;

            return type;
        }


        public virtual bool TypeIsExposedAsRepository(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return GetUriBaseType(type) != null;
        }


        public virtual bool TypeIsMapped(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return TypeIsMappedAsTransformedType(type) || TypeIsMappedAsSharedType(type) ||
                   IsNativelySupportedType(type)
                   || TypeIsMappedAsCollection(type)
                   || TypeIsAnonymousOrGrouping(type);
        }


        public virtual bool TypeIsMappedAsCollection(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            Type _;
            return type != typeof(string) &&
                   type.UniqueToken() != typeof(IGrouping<,>).UniqueToken() &&
                   type.TryGetEnumerableElementType(out _);
        }


        public virtual bool TypeIsMappedAsSharedType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return type.IsEnum || IsNativelySupportedType(type) || type == typeof(byte[])
                   || TypeIsMappedAsCollection(type) || typeof(QueryResult).IsAssignableFrom(type);
        }


        public virtual bool TypeIsMappedAsTransformedType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (type.IsEnum)
                return false;
            return SourceTypes.Contains(type) || TypeIsAnonymousOrGrouping(type);
        }


        public virtual bool TypeIsMappedAsValueObject(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return TypeIsAnonymousOrGrouping(type);
        }


        public virtual bool TypeIsSingletonResource(Type type)
        {
            // return type.HasAttribute<RootAttribute>(false);
            return false;
        }


        public IEnumerable<Attribute> GetPropertyAttributes(Type type, PropertyInfo propertyInfo)
        {
            return Enumerable.Empty<Attribute>();
        }


        public string GetTypeMappedName(Type type)
        {
            return type.Name;
        }


        private static bool TypeIsAnonymousOrGrouping(Type type)
        {
            return type.IsAnonymous() || TypeIsIGrouping(type);
        }


        private static bool TypeIsIGrouping(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return type.UniqueToken() == typeof(IGrouping<,>).UniqueToken();
        }

        #endregion
    }
}