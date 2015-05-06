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
using System.Linq;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.TypeSystem;
using Pomona.FluentMapping;

namespace Pomona.UnitTests.FluentMapping
{
    [TestFixture]
    public class FluentPropertyConfiguratorTests : FluentMappingTestsBase
    {
        [Test]
        public void AllowMethodForProperty_CombinesAllowedMethodWithConventionBasedPermissions()
        {
            CheckHowChangeInPropertyRuleAffectsFilter(x => x.Children,
                                                      x => x.Allow(HttpMethod.Delete),
                                                      (f, p) => f.GetPropertyAccessMode(p, null),
                                                      (origValue, changedValue) =>
                                                      {
                                                          Assert.That(changedValue,
                                                                      Is.Not.EqualTo(origValue),
                                                                      "Test no use if change in filter has no effect");
                                                          Assert.That(changedValue,
                                                                      Is.EqualTo(origValue | HttpMethod.Delete));
                                                      });
        }


        [Test]
        public void DenyMethodForProperty_RemovesMethodFromConventionBasedPermissions()
        {
            CheckHowChangeInPropertyRuleAffectsFilter(x => x.Children,
                                                      x => x.Deny(HttpMethod.Get),
                                                      (f, p) => f.GetPropertyAccessMode(p, null),
                                                      (origValue, changedValue) =>
                                                      {
                                                          Assert.That(changedValue,
                                                                      Is.Not.EqualTo(origValue),
                                                                      "Test no use if change in filter has no effect");
                                                          Assert.That(changedValue,
                                                                      Is.EqualTo(origValue & ~HttpMethod.Get));
                                                      });
        }


        [Test]
        public void Expand_ExtensionMethod_SetsExpandModeOfPropertyToFull()
        {
            CheckHowChangeInPropertyRuleAffectsFilter(x => x.Children,
                                                      x => x.Expand(),
                                                      (f, p) => f.GetPropertyExpandMode(p.ReflectedType, p),
                                                      (origValue, changedValue) =>
                                                      {
                                                          Assert.That(origValue, Is.EqualTo(ExpandMode.Default));
                                                          Assert.That(changedValue, Is.EqualTo(ExpandMode.Full));
                                                      });
        }


        [Test]
        public void Expand_TakingExpandModeArgument_SetsExpandModeOfProperty()
        {
            CheckHowChangeInPropertyRuleAffectsFilter(x => x.Children,
                                                      x => x.Expand(ExpandMode.Shallow),
                                                      (f, p) => f.GetPropertyExpandMode(p.ReflectedType, p),
                                                      (origValue, changedValue) =>
                                                      {
                                                          Assert.That(origValue, Is.EqualTo(ExpandMode.Default));
                                                          Assert.That(changedValue, Is.EqualTo(ExpandMode.Shallow));
                                                      });
        }


        [Test]
        public void ExpandShallow_ExtensionMethod_SetsExpandModeOfPropertyToShallow()
        {
            CheckHowChangeInPropertyRuleAffectsFilter(x => x.Children,
                                                      x => x.ExpandShallow(),
                                                      (f, p) => f.GetPropertyExpandMode(p.ReflectedType, p),
                                                      (origValue, changedValue) =>
                                                      {
                                                          Assert.That(origValue, Is.EqualTo(ExpandMode.Default));
                                                          Assert.That(changedValue, Is.EqualTo(ExpandMode.Shallow));
                                                      });
        }


        [Test]
        public void HasAttribute_AddsSpecifiedAttributeToDeclaredAttributesOfProperty()
        {
            CheckHowChangeInPropertyRuleAffectsFilter(x => x.Children,
                                                      x => x.HasAttribute(new ObsoleteAttribute()),
                                                      (f, p) => f.GetPropertyAttributes(p.ReflectedType, p),
                                                      (origValue, changedValue) =>
                                                      {
                                                          Assert.That(origValue.OfType<ObsoleteAttribute>().Any(),
                                                                      Is.False);
                                                          Assert.That(changedValue.OfType<ObsoleteAttribute>().Any(),
                                                                      Is.True);
                                                      });
        }


        [Test]
        public void ItemsAllowMethodForProperty_CombinesAllowedMethodWithConventionBasedPermissions()
        {
            CheckHowChangeInPropertyRuleAffectsFilter(x => x.Children,
                                                      x => x.ItemsAllow(HttpMethod.Delete),
                                                      (f, p) => f.GetPropertyItemAccessMode(p.ReflectedType, p),
                                                      (origValue, changedValue) =>
                                                      {
                                                          Assert.That(changedValue,
                                                                      Is.Not.EqualTo(origValue),
                                                                      "Test no use if change in filter has no effect");
                                                          Assert.That(changedValue,
                                                                      Is.EqualTo(origValue | HttpMethod.Delete));
                                                      });
        }


        [Test]
        public void ItemsDenyMethodForProperty_RemovesMethodFromConventionBasedPermissions()
        {
            CheckHowChangeInPropertyRuleAffectsFilter(x => x.Children,
                                                      x => x.ItemsDeny(HttpMethod.Get),
                                                      (f, p) => f.GetPropertyItemAccessMode(p.ReflectedType, p),
                                                      (origValue, changedValue) =>
                                                      {
                                                          Assert.That(changedValue,
                                                                      Is.Not.EqualTo(origValue),
                                                                      "Test no use if change in filter has no effect");
                                                          Assert.That(changedValue,
                                                                      Is.EqualTo(origValue & ~HttpMethod.Get));
                                                      });
        }


        [Test]
        public void PropertyOptionBuilder_Implementation_OverridesAllMethods()
        {
            // This is a sanity check using reflection.
            var assembly = typeof(IPropertyOptionsBuilder<,>).Assembly;
            var baseType =
                assembly.GetType("Pomona.FluentMapping.PropertyOptionsBuilderBase`2");
            Assert.That(baseType, Is.Not.Null);
            var implType =
                assembly.GetType("Pomona.FluentMapping.PropertyOptionsBuilder`2");
            Assert.That(implType, Is.Not.Null);
            var implMethods = implType.GetMethods();

            int failCount = 0;

            foreach (
                var baseMethod in
                    baseType.GetMethods().Where(x => x.IsVirtual && !x.IsFinal && x.DeclaringType == baseType))
            {
                var implMethod = implMethods.FirstOrDefault(x => x.GetBaseDefinition().IsGenericInstanceOf(baseMethod));
                if (implMethod == null)
                {
                    Console.WriteLine("Unable to find implementation of {0} on {1}.");
                    failCount++;
                    continue;
                }
                if (implMethod.DeclaringType != implType)
                {
                    Console.WriteLine("Method {0} has not been overrided by {1}.", baseMethod, implType);
                    failCount++;
                }
            }

            Assert.That(failCount, Is.EqualTo(0));
        }


        [Test]
        public void RenameRule_GivesPropertyANewName()
        {
            var fluentMappingFilter = GetMappingFilter();
            var propertyInfo = GetPropInfo<Top>(x => x.ToBeRenamed);
            Assert.That(
                fluentMappingFilter.GetPropertyMappedName(propertyInfo.ReflectedType, propertyInfo),
                Is.EqualTo("NewName"));
        }
    }
}