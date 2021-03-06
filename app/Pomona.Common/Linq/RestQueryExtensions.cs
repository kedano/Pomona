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
using System.Linq.Expressions;
using System.Reflection;

using Newtonsoft.Json.Linq;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Linq
{
    public static class RestQueryExtensions
    {
        public static IQueryable<TSource> Expand<TSource, TProperty>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TProperty>> propertySelector)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (propertySelector == null)
                throw new ArgumentNullException("propertySelector");

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(typeof(TSource), typeof(TProperty));
            var property = Expression.Quote(propertySelector);
            var methodCallExpression = Expression.Call(null,
                                                       genericMethod,
                                                       new[]
                                                       {
                                                           source.Expression,
                                                           property
                                                       });

            return source.Provider.CreateQuery<TSource>(methodCallExpression);
        }


        public static IEnumerable<TSource> Expand<TSource, TProperty>(this IEnumerable<TSource> source,
                                                                      Func<TSource, TProperty> propertySelector,
                                                                      ExpandMode expandMode)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (propertySelector == null)
                throw new ArgumentNullException("propertySelector");

            switch (expandMode)
            {
                case ExpandMode.Default:
                    return source;
                case ExpandMode.Full:
                    return source.Expand(propertySelector);
                case ExpandMode.Shallow:
                    return source.ExpandShallow(propertySelector);
                default:
                    throw new PomonaException("ExpandMode " + expandMode + "not recognized.");
            }
        }


        public static IQueryable<TSource> Expand<TSource, TProperty>(this IQueryable<TSource> source,
                                                                     Expression<Func<TSource, TProperty>>
                                                                         propertySelector,
                                                                     ExpandMode expandMode)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (propertySelector == null)
                throw new ArgumentNullException("propertySelector");

            switch (expandMode)
            {
                case ExpandMode.Default:
                    return source;
                case ExpandMode.Full:
                    return source.Expand(propertySelector);
                case ExpandMode.Shallow:
                    return source.ExpandShallow(propertySelector);
                default:
                    throw new PomonaException("ExpandMode " + expandMode + "not recognized.");
            }
        }


        public static IEnumerable<TSource> Expand<TSource, TProperty>(this IEnumerable<TSource> source,
                                                                      Func<TSource, TProperty> propertySelector)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (propertySelector == null)
                throw new ArgumentNullException("propertySelector");

            return source;
        }


        public static IEnumerable<TSource> ExpandShallow<TSource, TProperty>(
            this IEnumerable<TSource> source,
            Func<TSource, TProperty> propertySelector)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (propertySelector == null)
                throw new ArgumentNullException("propertySelector");

            return source;
        }


        /// <summary>
        /// Expands as list of references to resources. Only applicable to properties having a collection of resources.
        /// </summary>
        public static IQueryable<TSource> ExpandShallow<TSource, TProperty>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TProperty>> propertySelector)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (propertySelector == null)
                throw new ArgumentNullException("propertySelector");

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(typeof(TSource), typeof(TProperty));
            var property = Expression.Quote(propertySelector);
            var methodCallExpression = Expression.Call(null,
                                                       genericMethod,
                                                       new[]
                                                       {
                                                           source.Expression,
                                                           property
                                                       });

            return source.Provider.CreateQuery<TSource>(methodCallExpression);
        }


        public static TSource FirstLazy<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(new[] { typeof(TSource) });
            var methodCallExpression = Expression.Call(null, genericMethod, new[] { source.Expression });

            return source.Provider.Execute<TSource>(methodCallExpression);
        }


        public static TSource FirstLazy<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return source.First();
        }


        public static IQueryable<TSource> IncludeTotalCount<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(typeof(TSource));
            var methodCallExpression = Expression.Call(null, genericMethod, new[] { source.Expression });

            return source.Provider.CreateQuery<TSource>(methodCallExpression);
        }


        public static IEnumerable<TSource> IncludeTotalCount<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return source;
        }


        public static JToken ToJson<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(new[] { typeof(TSource) });
            var methodCallExpression = Expression.Call(null, genericMethod, new[] { source.Expression });

            return source.Provider.Execute<JObject>(methodCallExpression);
        }


        public static QueryResult<TSource> ToQueryResult<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(new[] { typeof(TSource) });
            var methodCallExpression = Expression.Call(null, genericMethod, new[] { source.Expression });

            return source.Provider.Execute<QueryResult<TSource>>(methodCallExpression);
        }


        public static QueryResult<TSource> ToQueryResult<TSource>(this IEnumerable<TSource> source)
        {
            var enumerable = source as TSource[] ?? source.ToArray();
            return new QueryResult<TSource>(enumerable, 0, enumerable.Length, null, null);
        }


        public static Uri ToUri<TSource>(this IQueryable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(new[] { typeof(TSource) });
            var methodCallExpression = Expression.Call(null, genericMethod, new[] { source.Expression });

            return source.Provider.Execute<Uri>(methodCallExpression);
        }


        public static IQueryable<TSource> WithOptions<TSource>(this IQueryable<TSource> source,
                                                               Action<IRequestOptions> optionsModifier)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var method = (MethodInfo)MethodBase.GetCurrentMethod();
            var genericMethod = method.MakeGenericMethod(typeof(TSource));

            var methodCallExpression = Expression.Call(null,
                                                       genericMethod,
                                                       new[]
                                                       {
                                                           source.Expression, Expression.Constant(optionsModifier)
                                                       });

            return source.Provider.CreateQuery<TSource>(methodCallExpression);
        }


        public static IEnumerable<TSource> WithOptions<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return source;
        }
    }
}