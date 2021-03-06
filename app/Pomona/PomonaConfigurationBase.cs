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

using Pomona.FluentMapping;
using Pomona.Routing;

namespace Pomona
{
    public abstract class PomonaConfigurationBase
    {
        public virtual IEnumerable<Delegate> FluentRuleDelegates
        {
            get { return Enumerable.Empty<Delegate>(); }
        }

        public virtual IEnumerable<object> FluentRuleObjects
        {
            get { return Enumerable.Empty<object>(); }
        }

        public virtual IEnumerable<Type> HandlerTypes
        {
            get { return Enumerable.Empty<Type>(); }
        }

        public virtual IEnumerable<IRouteActionResolver> RouteActionResolvers
        {
            get
            {
                return new[]
                {
                    new RequestHandlerActionResolver(),
                    DataSourceRouteActionResolver,
                    QueryGetActionResolver
                }.Where(x => x != null);
            }
        }

        public virtual IEnumerable<Type> SourceTypes
        {
            get { return new Type[] { }; }
        }

        public virtual ITypeMappingFilter TypeMappingFilter
        {
            get { return new DefaultTypeMappingFilter(SourceTypes); }
        }

        protected virtual Type DataSource
        {
            get { return typeof(IPomonaDataSource); }
        }

        protected virtual IRouteActionResolver DataSourceRouteActionResolver
        {
            get { return new DataSourceRouteActionResolver(); }
        }

        protected virtual IRouteActionResolver QueryGetActionResolver
        {
            get { return new QueryGetActionResolver(new DefaultQueryProviderCapabilityResolver()); }
        }


        public IPomonaSessionFactory CreateSessionFactory()
        {
            var typeMapper = new TypeMapper(this);
            return CreateSessionFactory(typeMapper);
        }


        public virtual void OnMappingComplete(TypeMapper typeMapper)
        {
        }


        protected virtual Route OnCreateRootRoute(TypeMapper typeMapper)
        {
            return new DataSourceRootRoute(typeMapper, DataSource);
        }


        internal FluentTypeMappingFilter CreateMappingFilter()
        {
            var innerFilter = TypeMappingFilter;
            var fluentRuleObjects = FluentRuleObjects.ToArray();
            var fluentFilter = new FluentTypeMappingFilter(innerFilter, fluentRuleObjects, FluentRuleDelegates, SourceTypes);
            var wrappableFilter = innerFilter as IWrappableTypeMappingFilter;
            if (wrappableFilter != null)
                wrappableFilter.BaseFilter = fluentFilter;
            return fluentFilter;
        }


        internal IPomonaSessionFactory CreateSessionFactory(TypeMapper typeMapper)
        {
            var pomonaSessionFactory = new PomonaSessionFactory(typeMapper, OnCreateRootRoute(typeMapper),
                                                                new InternalRouteActionResolver(RouteActionResolvers));
            return pomonaSessionFactory;
        }
    }
}