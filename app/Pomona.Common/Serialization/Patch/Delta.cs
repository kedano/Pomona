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

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Patch
{
    public abstract class Delta
    {
        protected Delta()
        {
        }


        protected Delta(object original, TypeSpec type, ITypeResolver typeMapper, Delta parent = null)
        {
            if (original == null)
                throw new ArgumentNullException("original");
            if (type == null)
                throw new ArgumentNullException("type");
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            Original = original;
            Type = type;
            TypeMapper = typeMapper;
            this.Parent = parent;
        }


        public bool IsDirty { get; private set; }
        public object Original { get; internal set; }
        public Delta Parent { get; internal set; }
        public TypeSpec Type { get; internal set; }
        public ITypeResolver TypeMapper { get; internal set; }
        public abstract void Apply();


        public virtual void Reset()
        {
            this.IsDirty = false;
        }


        public virtual void SetDirty()
        {
            if (this.IsDirty)
                return;

            this.IsDirty = true;
            if (this.Parent != null)
                this.Parent.SetDirty();
        }


        protected void ClearDirty()
        {
            this.IsDirty = false;
        }


        protected virtual object CreateNestedDelta(object propValue, TypeSpec propValueType, Type propertyType)
        {
            if (propValueType.SerializationMode == TypeSerializationMode.Structured)
                return new ObjectDelta(propValue, propValueType, TypeMapper, this);
            if (propValueType.IsCollection)
                return new CollectionDelta(propValue, propValueType, TypeMapper, this);
            throw new NotImplementedException();
        }


        protected static void DetachFromParent(object oldValue)
        {
            var oldDeltaValue = oldValue as Delta;
            if (oldDeltaValue != null)
                oldDeltaValue.Parent = null;
        }


        protected static bool ValueIsDirty(object o)
        {
            var delta = o as IDelta;
            return delta == null || delta.IsDirty;
        }
    }
}