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

using System.Text;

using Newtonsoft.Json;

namespace Pomona.Common.Web
{
    [JsonConverter(typeof(HttpResponseConverter))]
    public class HttpResponse
    {
        private readonly byte[] body;
        private readonly HttpHeaders headers;
        private readonly string protocolVersion;
        private readonly HttpStatusCode statusCode;


        public HttpResponse(HttpStatusCode statusCode, byte[] body = null, HttpHeaders headers = null, string protocolVersion = "1.1")
        {
            this.headers = headers ?? new HttpHeaders();
            this.body = body;
            this.statusCode = statusCode;
            this.protocolVersion = protocolVersion;
        }


        public byte[] Body
        {
            get { return this.body; }
        }

        public HttpHeaders Headers
        {
            get { return this.headers; }
        }

        public HttpStatusCode StatusCode
        {
            get { return this.statusCode; }
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("HTTP/{0} {1} {2}\r\n", this.protocolVersion, (int)this.statusCode, this.statusCode);
            foreach (var h in this.headers)
            {
                foreach (var v in h.Value)
                    sb.AppendFormat("{0}: {1}\r\n", h.Key, v);
            }
            sb.AppendLine();

            if (this.body != null)
                sb.Append(Encoding.UTF8.GetString(this.body));
            sb.AppendLine();
            return sb.ToString();
        }
    }
}