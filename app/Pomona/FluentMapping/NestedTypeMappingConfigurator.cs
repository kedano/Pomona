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
using System.Linq.Expressions;

namespace Pomona.FluentMapping
{
    internal class NestedTypeMappingConfigurator<TDeclaring> : TypeMappingConfiguratorBase<TDeclaring>
    {
        private readonly List<Delegate> typeConfigurationDelegates = new List<Delegate>();


        public NestedTypeMappingConfigurator(List<Delegate> typeConfigurationDelegates)
        {
            this.typeConfigurationDelegates = typeConfigurationDelegates;
        }


        protected override ITypeMappingConfigurator<TDeclaring> OnHasChild<TItem>(
            Expression<Func<TDeclaring, TItem>> childProperty,
            Expression<Func<TItem, TDeclaring>> parentProperty,
            Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> typeOptions,
            Func<IPropertyOptionsBuilder<TDeclaring, TItem>, IPropertyOptionsBuilder<TDeclaring, TItem>>
                propertyOptions)
        {
            Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> asChildResourceMapping =
                x => x.AsChildResourceOf(parentProperty, childProperty);
            this.typeConfigurationDelegates.Add(asChildResourceMapping);
            this.typeConfigurationDelegates.Add(typeOptions);
            return this;
        }


        protected override ITypeMappingConfigurator<TDeclaring> OnHasChildren<TItem>(
            Expression<Func<TDeclaring, IEnumerable<TItem>>> collectionProperty,
            Expression<Func<TItem, TDeclaring>> parentProperty,
            Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> typeOptions,
            Func
                <IPropertyOptionsBuilder<TDeclaring, IEnumerable<TItem>>,
                IPropertyOptionsBuilder<TDeclaring, IEnumerable<TItem>>> propertyOptions)
        {
            Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> asChildResourceMapping =
                x => x.AsChildResourceOf(parentProperty, collectionProperty);
            this.typeConfigurationDelegates.Add(asChildResourceMapping);
            this.typeConfigurationDelegates.Add(typeOptions);
            return this;
        }
    }
}