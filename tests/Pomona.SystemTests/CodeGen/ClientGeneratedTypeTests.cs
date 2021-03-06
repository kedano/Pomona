﻿#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Critters.Client;

using Mono.Cecil;

using NUnit.Framework;

using Pomona.Common;
using Pomona.UnitTests;

namespace Pomona.SystemTests.CodeGen
{
    [TestFixture]
    public class ClientGeneratedTypeTests : ClientTestsBase
    {
        [Test]
        public void AbstractClassOnServerIsAbstractOnClient()
        {
            Assert.That(typeof(AbstractAnimalForm).IsAbstract);
        }


        [Test]
        public void AllInterfacesArePrefixedByLetterI()
        {
            foreach (var t in typeof(CritterClient).Assembly.GetTypes().Where(x => x.IsInterface))
            {
                try
                {
                    Assert.That(t.Name.Length, Is.GreaterThan(1));
                    Assert.That(t.Name[0], Is.EqualTo('I'));
                    Assert.That(char.IsUpper(t.Name[1]), Is.True);
                }
                catch (AssertionException)
                {
                    Console.WriteLine("Failed while testing type " + t.FullName);
                    throw;
                }
            }
        }


        [Test]
        public void AllPropertyTypesOfClientTypesAreAllowed()
        {
            var clientAssembly = typeof(ICritter).Assembly;
            var allPropTypes =
                clientAssembly.GetExportedTypes().SelectMany(
                    x => x.GetProperties().Select(y => y.PropertyType)).Distinct();

            var allTypesOk = true;
            foreach (var type in allPropTypes)
            {
                if (!IsAllowedType(type))
                {
                    allTypesOk = false;
                    var typeLocal = type;
                    var propsWithType = clientAssembly
                        .GetExportedTypes()
                        .SelectMany(x => x.GetProperties())
                        .Where(x => x.PropertyType == typeLocal).ToList();
                    foreach (var propertyInfo in propsWithType)
                    {
                        Console.WriteLine(
                            "Property {0} of {1} has type {2} of assembly {3}, which should not be referenced by client!",
                            propertyInfo.Name,
                            propertyInfo.DeclaringType.FullName,
                            propertyInfo.PropertyType.FullName,
                            propertyInfo.PropertyType.Assembly.FullName);
                    }
                }
            }

            Assert.IsTrue(allTypesOk, "There was properties in CritterClient with references to disallowed assemblies.");
        }


        [Test]
        public void AssemblyVersionSetToApiVersionFromTypeMappingFilter()
        {
            Assert.That(typeof(CritterClient).Assembly.GetName().Version.ToString(3), Is.EqualTo("0.1.0"));
        }


        [Test]
        public void ClientLibraryIsCorrectlyGenerated()
        {
            var foundError = false;
            var errors = new StringBuilder();
            foreach (
                var prop in
                    Client.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(
                        x =>
                            x.PropertyType.IsGenericType
                            && x.PropertyType.GetGenericTypeDefinition() == typeof(ClientRepository<,,>)))
            {
                var value = prop.GetValue(Client, null);
                if (value == null)
                {
                    foundError = true;
                    errors.AppendFormat("Property {0} of generated client lib is null\r\n", prop.Name);
                }
                if (prop.GetSetMethod(true).IsPublic)
                {
                    foundError = true;
                    errors.AppendFormat("Property {0} of generated client lib has a public setter.\r\n", prop.Name);
                }
            }

            if (foundError)
                Assert.Fail("Found the following errors on generated client lib: {0}\r\n", errors);
        }


        [Test]
        public void GeneratedPocoTypeInitializesDictionaryPropertyInConstructor()
        {
            var dictContainer = new DictionaryContainerResource();
            Assert.That(dictContainer.Map, Is.Not.Null);
        }


        [Test]
        public void GeneratedPocoTypeInitializesListPropertyInConstructor()
        {
            var critter = new CritterResource();
            Assert.That(critter.Weapons, Is.Not.Null);
        }


        [Test]
        public void GeneratedPocoTypeInitializesValueObjectPropertyInConstructor()
        {
            var critter = new CritterResource();
            Assert.That(critter.CrazyValue, Is.Not.Null);
        }


        [Test]
        public void GeneratedPropertyHasResourcePropertyAttributeWithAccessDeclared()
        {
            var prop = typeof(ICritter).GetProperty("Name");
            Assert.That(prop, Is.Not.Null);
            var attr = prop.GetCustomAttributes(true).OfType<ResourcePropertyAttribute>().First();
            Assert.That(attr.AccessMode, Is.EqualTo(HttpMethod.Post | HttpMethod.Put | HttpMethod.Get | HttpMethod.Patch));
        }


        [Test]
        public void MiddleBaseClassExcludedFromMapping_WillBeExcludedInGeneratedClient()
        {
            Assert.That(typeof(IInheritsFromHiddenBase).GetInterfaces(), Has.Member(typeof(IEntityBase)));
            Assert.That(typeof(CritterClient).Assembly.GetTypes().Count(x => x.Name == "IHiddenBaseInMiddle"), Is.EqualTo(0));
            Assert.That(typeof(IInheritsFromHiddenBase).GetProperty("ExposedFromDerivedResource"), Is.Not.Null);
        }


        [Test]
        public void NameOfGeneratedTypeFromInterfaceDoesNotGetDoubleIPrefix()
        {
            Assert.That(typeof(ICritter).Assembly.GetTypes().Any(x => x.Name == "IIExposedInterface"), Is.False);
            Assert.That(typeof(ICritter).Assembly.GetTypes().Any(x => x.Name == "IExposedInterface"), Is.True);
        }


        [Test]
        public void NoClassesArePrefixedWithTheLetterI()
        {
            foreach (var t in typeof(CritterClient).Assembly.GetTypes().Where(x => !x.IsInterface))
            {
                try
                {
                    Assert.That(t.Name.Length, Is.GreaterThan(1));
                    Assert.That(char.IsUpper(t.Name[0]), Is.True);
                    Assert.That(char.IsLower(t.Name[1]), Is.True);
                }
                catch (AssertionException)
                {
                    Console.WriteLine("Failed while testing type " + t.FullName);
                    throw;
                }
            }
        }


        [Test]
        public void ObsoletePropertyIsCopiedFromServerProperty()
        {
            Assert.That(
                typeof(ICritter).GetProperty("ObsoletedProperty").GetCustomAttributes(true).OfType<ObsoleteAttribute>(),
                Is.Not.Empty);
        }


        [Category("WindowsRequired")]
        [Test]
        public void PeVerify_ClientWithEmbeddedPomonaCommon_HasExitCode0()
        {
            var origDllPath = typeof(ICritter).Assembly.Location;
            var dllDir = Path.GetDirectoryName(origDllPath);
            var clientWithEmbeddedStuffName = Path.Combine(dllDir, "../../../../lib/IndependentCritters.dll");
            var newDllPath = Path.Combine(dllDir, "TempCopiedIndependentCrittersDll.tmp");
            File.Copy(clientWithEmbeddedStuffName, newDllPath, true);
            PeVerify(newDllPath);
        }


        [Category("WindowsRequired")]
        [Test]
        public void PeVerify_HasExitCode0()
        {
            PeVerify(typeof(ICritter).Assembly.Location);
        }


        [Category("WindowsRequired")]
        [Test(Description = "This test has been added since more errors are discovered when dll has been renamed.")]
        public void PeVerify_RenamedToAnotherDllName_StillHasExitCode0()
        {
            var origDllPath = typeof(ICritter).Assembly.Location;
            Console.WriteLine(Path.GetDirectoryName(origDllPath));
            var newDllPath = Path.Combine(Path.GetDirectoryName(origDllPath), "TempCopiedClientLib.tmp");
            File.Copy(origDllPath, newDllPath, true);
            PeVerify(newDllPath);
            //Assert.Inconclusive();
        }


        [Ignore("This test is only applicable when DISABLE_PROXY_GENERATION is set.")]
        [Test]
        public void PomonaCommonHaveZeroReferencesToEmitNamespace()
        {
            var assembly = AssemblyDefinition.ReadAssembly(typeof(IClientResource).Assembly.Location);
            var trefs =
                assembly.MainModule.GetTypeReferences().Where(x => x.Namespace == "System.Reflection.Emit").ToList();
            Assert.That(trefs, Is.Empty);
        }


        [Test]
        public void PropertyGeneratedFromInheritedVirtualProperty_IsNotDuplicatedOnInheritedInterface()
        {
            Assert.That(typeof(IAbstractAnimal).GetProperty("TheVirtualProperty"), Is.Not.Null);
            Assert.That(typeof(IBear).GetProperty("TheVirtualProperty"), Is.EqualTo(null));
            Assert.That(typeof(IAbstractAnimal).GetProperty("TheAbstractProperty"), Is.Not.Null);
            Assert.That(typeof(IBear).GetProperty("TheAbstractProperty"), Is.EqualTo(null));
        }


        [Test]
        public void PropertyOfPostForm_ThatIsPublicWritableOnServer_AndReadOnlyThroughApi_IsNotPublic()
        {
            Assert.That(typeof(CritterForm).GetProperty("PublicAndReadOnlyThroughApi"), Is.Null);
        }


        [Test]
        public void PropertyOfPostFormOfAbstractType_ThatIsPublicWritableOnServer_AndReadOnlyThroughApi_IsNotPublic()
        {
            Assert.That(typeof(AbstractAnimalForm).GetProperty("PublicAndReadOnlyThroughApi"), Is.Null);
        }


        [Test]
        public void ResourceInfoAttributeOfGeneratedTypeHasCorrectEtagPropertySet()
        {
            var resInfo = typeof(IEtaggedEntity).GetCustomAttributes(false).OfType<ResourceInfoAttribute>().First();
            Assert.That(resInfo.EtagProperty, Is.EqualTo(typeof(IEtaggedEntity).GetProperty("ETag")));
        }


        [Test]
        public void ResourceInfoAttributeOfGeneratedTypeHasCorrectIdPropertySet()
        {
            var resInfo = typeof(ICritter).GetCustomAttributes(false).OfType<ResourceInfoAttribute>().First();
            Assert.That(resInfo.EtagProperty, Is.EqualTo(typeof(IEtaggedEntity).GetProperty("Id")));
        }


        [Test]
        public void ResourceInfoAttributeOfGeneratedTypeHasParentResourceTypeSet()
        {
            var resInfo = typeof(IPlanet).GetCustomAttributes(false).OfType<ResourceInfoAttribute>().First();
            Assert.That(resInfo.ParentResourceType, Is.EqualTo(typeof(IPlanetarySystem)));
        }


        [Test]
        public void ResourceInheritedFromResourceWithPostDeniedDoesNotHavePostResourceFormGenerated()
        {
            var typeInfo =
                typeof(IInheritedUnpostableThing).GetCustomAttributes(false).OfType<ResourceInfoAttribute>().First();
            Assert.That(typeInfo.PostFormType, Is.Null);
            Assert.That(
                typeof(IInheritedUnpostableThing).Assembly.GetType("Critters.Client.InheritedUnpostableThingForm"),
                Is.Null);
        }


        [Test]
        public void ResourceWithPostDeniedDoesNotHavePostResourceFormGenerated()
        {
            var typeInfo = typeof(IUnpostableThing).GetCustomAttributes(false).OfType<ResourceInfoAttribute>().First();
            Assert.That(typeInfo.PostFormType, Is.Null);
            Assert.That(typeof(IUnpostableThing).Assembly.GetType("Critters.Client.UnpostableThingForm"), Is.Null);
            Assert.That(typeof(IUnpostableThing).Assembly.GetType("Critters.Client.CritterForm"), Is.Not.Null);
        }


        [Test]
        public void ThingIndependentFromBase_DoesNotInheritEntityBase()
        {
            Assert.That(!typeof(IEntityBase).IsAssignableFrom(typeof(IThingIndependentFromBase)));
        }


        [Test]
        public void ThingIndependentFromBase_IncludesPropertyFromEntityBase()
        {
            Assert.That(typeof(IThingIndependentFromBase).GetProperty("Id"), Is.Not.Null);
        }


        private static void PeVerify(string dllPath)
        {
            PeVerifyHelper.Verify(dllPath);
        }
    }
}