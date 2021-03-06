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
using System.Collections.Generic;
using System.Linq;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Queries
{
    public class QueryTypeResolver : IQueryTypeResolver
    {
        private static readonly Dictionary<string, Type> nativeTypes =
            TypeUtils.GetNativeTypes().ToDictionary(x => x.Name.ToLower(), x => x);

        private readonly ITypeResolver typeMapper;


        public QueryTypeResolver(ITypeResolver typeMapper)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.typeMapper = typeMapper;
        }

        #region Implementation of IQueryTypeResolver

        public bool TryResolveProperty<TProperty>(Type type, string propertyPath, out TProperty property) where TProperty : PropertySpec
        {
            // TODO: Proper exception handling when type is not TransformedType [KNS]
            property = null;
            var transformedType = (StructuredType)this.typeMapper.FromType(type);
            PropertySpec uncastProperty;
            return transformedType.TryGetPropertyByName(propertyPath,
                                                        StringComparison.InvariantCultureIgnoreCase,
                                                        out uncastProperty) && (property = uncastProperty as TProperty) != null;
        }


        public Type ResolveType(string typeName)
        {
            Type type;

            if (typeName.EndsWith("?"))
                return typeof(Nullable<>).MakeGenericType(ResolveType(typeName.Substring(0, typeName.Length - 1)));

            if (nativeTypes.TryGetValue(typeName.ToLower(), out type))
                return type;

            return this.typeMapper.FromType(typeName).Type;
        }

        #endregion
    }
}