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
using System.Globalization;
using System.Linq;

namespace Pomona.Queries
{
    internal class NumberNode : NodeBase
    {
        private readonly string value;


        public NumberNode(string value)
            : base(NodeType.NumberLiteral, Enumerable.Empty<NodeBase>())
        {
            this.value = value;
        }


        public string Value
        {
            get { return this.value; }
        }


        public object Parse()
        {
            var lastCharacter = this.value[this.value.Length - 1];
            if (lastCharacter == 'm' || lastCharacter == 'M')
                return decimal.Parse(this.value.Substring(0, this.value.Length - 1), CultureInfo.InvariantCulture);
            if (lastCharacter == 'f' || lastCharacter == 'F')
                return float.Parse(this.value.Substring(0, this.value.Length - 1), CultureInfo.InvariantCulture);
            if (lastCharacter == 'L')
                return Int64.Parse(this.value.Substring(0, this.value.Length - 1), CultureInfo.InvariantCulture);

            var parts = this.value.Split('.');
            if (parts.Length == 1)
                return int.Parse(parts[0], CultureInfo.InvariantCulture);
            if (parts.Length == 2)
                return double.Parse(this.value, CultureInfo.InvariantCulture);

            throw new InvalidOperationException("Unable to parse " + this.value);
        }


        public override string ToString()
        {
            return base.ToString() + " " + Value;
        }
    }
}