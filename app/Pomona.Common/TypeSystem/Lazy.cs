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

namespace Pomona.Common.TypeSystem
{
#if CUSTOM_LAZY_TYPE
    internal class Lazy<T>
    {
        [ThreadStatic]
        private static int recursiveCallCounter;

        private readonly Func<T> factory;
        private readonly LazyThreadSafetyMode lazyThreadSafetyMode;
        private bool isInitialized;
        private T value;


        public Lazy(Func<T> factory, LazyThreadSafetyMode lazyThreadSafetyMode)
        {
            this.lazyThreadSafetyMode = lazyThreadSafetyMode;
            this.factory = factory ?? Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();
        }


        public T Value
        {
            get
            {
                if (!this.isInitialized)
                {
                    try
                    {
                        if (recursiveCallCounter++ > 500)
                        {
                            throw new InvalidOperationException(
                                "Seems like we're going to get a StackOverflowException here, lets fail early to avoid that.");
                        }
                        this.value = this.factory();
                    }
                    finally
                    {
                        recursiveCallCounter--;
                    }
                    Thread.MemoryBarrier();
                    this.isInitialized = true;
                }
                return this.value;
            }
        }
    }
#endif
}