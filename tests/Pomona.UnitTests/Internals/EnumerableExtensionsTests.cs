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

using Pomona.Common.Internals;

namespace Pomona.UnitTests.Internals
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        [Test]
        public void Pad_2To7_Return7()
        {
            var enumerable = new[] { "0", "1" };
            var result = enumerable.Pad(7, "0").ToArray();

            Assert.That(result, Has.Length.EqualTo(7));
        }


        [Test]
        public void Pad_EmptyTo4_Return4()
        {
            var enumerable = Enumerable.Empty<string>();
            var result = enumerable.Pad(4, "0").ToArray();

            Assert.That(result, Has.Length.EqualTo(4));
        }
    }
}