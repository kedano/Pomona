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

using System.Linq;

using NUnit.Framework;

using Pomona.Common.TypeSystem;
using Pomona.Example;
using Pomona.Example.Models;
using Pomona.Example.Models.Existence;

namespace Pomona.UnitTests.TypeSystem
{
    [TestFixture]
    public class TypeSystemTests
    {
        [Test]
        public void FromEnumType_ReturnsEnumTypeSpec()
        {
            var mapper = new TypeMapper(new CritterPomonaConfiguration());
            Assert.That(mapper.FromType<CustomEnum>(), Is.InstanceOf<EnumTypeSpec>());
        }


        [Test]
        public void TransformedType_RequiredProperties_ReturnsRequiredProperties()
        {
            var mapper = new TypeMapper(new CritterPomonaConfiguration());
            var type = mapper.FromType(typeof(Planet));
            var requiredProperties = type.RequiredProperties.ToList();
            Assert.That(requiredProperties.All(x => x.IsRequiredForConstructor), Is.True);
            Assert.Inconclusive();
        }
    }
}