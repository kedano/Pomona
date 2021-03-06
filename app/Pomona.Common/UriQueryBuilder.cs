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
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Pomona.Common.Internals;
using Pomona.Common.Linq;

namespace Pomona.Common
{
    internal class UriQueryBuilder
    {
        private readonly StringBuilder stringBuilder = new StringBuilder();


        public void AppendExpressionParameter(string queryKey, Expression expression)
        {
            AppendExpressionParameter<QueryPredicateBuilder>(queryKey, expression);
        }


        public void AppendExpressionParameter<TVisitor>(string queryKey, Expression expression)
            where TVisitor : ExpressionVisitor, new()
        {
            var pomonaExpression = (PomonaExtendedExpression)expression.Visit<TVisitor>();
            if (!pomonaExpression.SupportedOnServer)
            {
                var unsupportedExpressions = pomonaExpression.WrapAsEnumerable()
                                                             .Flatten(x => x.Children.OfType<PomonaExtendedExpression>())
                                                             .OfType<NotSupportedByProviderExpression>().ToList();

                if (unsupportedExpressions.Count == 1)
                    throw unsupportedExpressions[0].Exception;

                throw new AggregateException(unsupportedExpressions.Select(x => x.Exception));
            }
            var filterString = pomonaExpression.ToString();

            AppendQueryParameterStart(queryKey);
            AppendEncodedQueryValue(filterString);
        }


        public void AppendParameter(string key, object value)
        {
            AppendQueryParameterStart(key);
            AppendEncodedQueryValue(value.ToString());
        }


        public override string ToString()
        {
            return this.stringBuilder.ToString();
        }


        private void AppendEncodedQueryValue(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var sb = this.stringBuilder;

            foreach (var b in bytes)
            {
                if (b == ' ')
                    sb.Append('+');
                else if (b < 128
                         &&
                         (char.IsLetterOrDigit((char)b) || b == '\'' || b == '.' || b == '~' || b == '-' || b == '_'
                          || b == ')' || b == '(' || b == ' ' || b == '$'))
                    sb.Append((char)b);
                else
                    sb.AppendFormat("%{0:X2}", b);
            }
        }


        private void AppendQueryParameterStart(string queryKey)
        {
            if (this.stringBuilder.Length > 0)
                this.stringBuilder.Append('&');

            AppendEncodedQueryValue(queryKey);
            this.stringBuilder.Append('=');
        }
    }
}