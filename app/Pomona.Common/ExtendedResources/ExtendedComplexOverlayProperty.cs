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
using System.Reflection;

using Pomona.Common.Proxies;

namespace Pomona.Common.ExtendedResources
{
    internal class ExtendedComplexOverlayProperty : ExtendedOverlayProperty
    {
        public ExtendedComplexOverlayProperty(PropertyInfo property,
                                              PropertyInfo underlyingProperty,
                                              ExtendedResourceInfo info)
            : base(property, underlyingProperty, info)
        {
        }


        public override object GetValue(object obj, IDictionary<string, IExtendedResourceProxy> cache)
        {
            IExtendedResourceProxy extendedResource;
            var underlyingResource = UnderlyingProperty.GetValue(obj, null);

            if (TryGetFromCache(cache, underlyingResource, out extendedResource))
                return extendedResource;

            if (underlyingResource != null)
            {
                extendedResource =
                    (IExtendedResourceProxy)
                        RuntimeProxyFactory.Create(typeof(ExtendedResourceBase), Property.PropertyType);
                var proxyBase = (ExtendedResourceBase)extendedResource;
                proxyBase.Initialize(ClientTypeResolver.Default, Info, underlyingResource);
            }
            else
                extendedResource = null;
            cache[Property.Name] = extendedResource;
            return extendedResource;
        }


        public override void SetValue(object obj, object value, IDictionary<string, IExtendedResourceProxy> cache)
        {
            if (value == null)
            {
                cache.Remove(Property.Name);
                UnderlyingProperty.SetValue(obj, null, null);
                return;
            }
            var extendedResource = (IExtendedResourceProxy)value;
            var underlyingResource = extendedResource.WrappedResource;
            UnderlyingProperty.SetValue(obj, underlyingResource, null);
            cache[Property.Name] = extendedResource;
        }
    }
}