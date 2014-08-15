﻿#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Pomona.Common.Linq
{
    internal class NotSupportedByProviderExpression : PomonaExtendedExpression
    {
        private readonly Exception exception;
        private readonly Expression expression;


        public NotSupportedByProviderExpression(Expression expression, Exception exception = null) : base(expression.Type)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            this.expression = expression;
            this.exception = exception;
        }


        public override bool LocalExecutionPreferred
        {
            get { return true; }
        }

        public Expression Expression
        {
            get { return this.expression; }
        }

        public override ExpressionType NodeType
        {
            get { return ExpressionType.Extension; }
        }

        public override string ToString()
        {
            return "(☹ node \"" + expression + "\" not supported. ]])";
        }


        public override bool SupportedOnServer
        {
            get { return false; }
        }

        public override ReadOnlyCollection<object> Children
        {
            get { return new ReadOnlyCollection<object>(new object[] { }); }
        }
    }
}