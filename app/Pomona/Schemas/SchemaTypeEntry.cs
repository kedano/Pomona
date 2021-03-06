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

using System.Collections.Generic;

using Newtonsoft.Json;

using Pomona.Common;

namespace Pomona.Schemas
{
    public class SchemaTypeEntry
    {
        public SchemaTypeEntry()
        {
            Properties = new Dictionary<string, SchemaPropertyEntry>();
        }


        public bool Abstract { get; set; }

        [JsonIgnore]
        public HttpMethod AllowedMethods { get; set; }

        [JsonProperty(PropertyName = "access")]
        public string[] AllowedMethodsAsArray
        {
            get { return AllowedMethods != 0 ? Schema.HttpAccessModeToMethodsArray(AllowedMethods) : null; }
            set { AllowedMethods = Schema.MethodsArrayToHttpAccessMode(value); }
        }

        public string Extends { get; set; }
        public string Name { get; set; }
        public IDictionary<string, SchemaPropertyEntry> Properties { get; set; }
        public string Uri { get; set; }
    }
}