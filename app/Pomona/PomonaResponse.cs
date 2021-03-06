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

using Nancy;

using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class PomonaResponse<T> : PomonaResponse
    {
        public PomonaResponse(T entity,
                              HttpStatusCode statusCode = HttpStatusCode.OK,
                              string expandedPaths = "",
                              TypeSpec resultType = null,
                              IEnumerable<KeyValuePair<string, string>> responseHeaders = null)
            : base(entity, statusCode, expandedPaths, resultType, responseHeaders)
        {
        }


        public PomonaResponse(PomonaQuery query, T entity)
            : base(query, entity)
        {
        }


        public PomonaResponse(PomonaQuery query, T entity, HttpStatusCode statusCode)
            : base(query, entity, statusCode)
        {
        }


        public new T Entity
        {
            get { return (T)base.Entity; }
        }
    }

    public class PomonaResponse
    {
        internal static readonly object NoBodyEntity;
        private readonly object entity;
        private readonly string expandedPaths;
        private readonly List<KeyValuePair<string, string>> responseHeaders;
        private readonly TypeSpec resultType;
        private readonly HttpStatusCode statusCode;


        static PomonaResponse()
        {
            NoBodyEntity = new object();
        }


        public PomonaResponse(PomonaContext context,
                              object entity,
                              HttpStatusCode statusCode = HttpStatusCode.OK,
                              string expandedPaths = "",
                              TypeSpec resultType = null,
                              IEnumerable<KeyValuePair<string, string>> responseHeaders = null)
            : this(entity, statusCode, GetExpandedPaths(context, expandedPaths), resultType, responseHeaders)
        {
        }


        public PomonaResponse(PomonaQuery query, object entity)
            : this(query, entity, HttpStatusCode.OK)
        {
        }


        public PomonaResponse(object entity,
                              HttpStatusCode statusCode = HttpStatusCode.OK,
                              string expandedPaths = "",
                              TypeSpec resultType = null,
                              IEnumerable<KeyValuePair<string, string>> responseHeaders = null)
        {
            this.entity = entity;
            this.statusCode = statusCode;
            this.expandedPaths = expandedPaths;
            this.resultType = resultType;

            if (responseHeaders != null)
                this.responseHeaders = responseHeaders.ToList();
        }


        public PomonaResponse(PomonaQuery query, object entity, HttpStatusCode statusCode)
        {
            if (query == null)
                throw new ArgumentNullException("query");

            this.entity = entity;
            this.statusCode = statusCode;
            this.expandedPaths = query.ExpandedPaths;
            this.resultType = query.ResultType;
        }


        public object Entity
        {
            get { return this.entity; }
        }

        public string ExpandedPaths
        {
            get { return this.expandedPaths; }
        }

        public List<KeyValuePair<string, string>> ResponseHeaders
        {
            get { return this.responseHeaders; }
        }

        public TypeSpec ResultType
        {
            get { return this.resultType; }
        }

        public HttpStatusCode StatusCode
        {
            get { return this.statusCode; }
        }


        private static string GetExpandedPaths(PomonaContext context, string expandedPaths)
        {
            return String.IsNullOrEmpty(expandedPaths) && context != null
                ? context.ExpandedPaths
                : expandedPaths;
        }
    }
}