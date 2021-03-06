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

using Nancy;

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.Routing;

namespace Pomona.RequestProcessing
{
    public abstract class HandlerMethodInvoker<TInvokeState> : RouteAction, IHandlerMethodInvoker
        where TInvokeState : class, new()
    {
        private readonly HandlerMethod method;


        protected HandlerMethodInvoker(HandlerMethod method)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            this.method = method;
        }


        public HandlerMethod Method
        {
            get { return this.method; }
        }

        public IList<HandlerParameter> Parameters
        {
            get { return Method.Parameters; }
        }


        public override bool CanProcess(PomonaContext context)
        {
            return true;
        }


        public override PomonaResponse Process(PomonaContext context)
        {
            return InvokeAndWrap(context);
        }


        protected virtual object OnGetArgument(HandlerParameter parameter, PomonaContext context, TInvokeState state)
        {
            if (parameter.IsResource)
            {
                var parentNode = context
                    .Node
                    .Ascendants()
                    .FirstOrDefault(x => x.ResultType == parameter.TypeSpec);
                if (parentNode != null)
                    return parentNode.Value;
            }

            if (parameter.Type == typeof(PomonaContext))
                return context;

            Exception innerEx = null;
            try
            {
                // Never get value of transformed type parameter from IOC container
                if (!parameter.Type.IsValueType && !parameter.IsTransformedType)
                    return context.Session.GetInstance(parameter.Type);
            }
            catch (Exception ex)
            {
                innerEx = ex;
            }
            throw new HandlerMethodInvocationException(context,
                                                       this,
                                                       string.Format(
                                                           "Unable to invoke handler {0}.{1}, don't know how to provide value for parameter {2}",
                                                           Method.MethodInfo.ReflectedType,
                                                           Method.Name,
                                                           parameter.Name),
                                                       innerEx);
        }


        protected virtual object OnInvoke(object target, PomonaContext context, TInvokeState state)
        {
            var args = new object[Parameters.Count];

            for (var i = 0; i < Parameters.Count; i++)
            {
                //else if (resourceIdArg != null && p.Type == resourceIdArg.GetType())
                //    args[i] = resourceIdArg;
                //else
                args[i] = OnGetArgument(Parameters[i], context, state);
            }

            return Method.MethodInfo.Invoke(target, args);
        }


        private PomonaResponse InvokeAndWrap(PomonaContext context,
                                             HttpStatusCode? statusCode = null)
        {
            var handler = context.Session.GetInstance(this.method.MethodInfo.ReflectedType);
            var result = Invoke(handler, context);
            var resultAsResponse = result as PomonaResponse;
            if (resultAsResponse != null)
                return resultAsResponse;

            var responseBody = result;
            if (ReturnType == typeof(void))
                responseBody = PomonaResponse.NoBodyEntity;

            if (responseBody == PomonaResponse.NoBodyEntity)
                statusCode = HttpStatusCode.NoContent;

            return new PomonaResponse(context, responseBody, statusCode ?? HttpStatusCode.OK, context.ExpandedPaths);
        }


        public virtual object Invoke(object target, PomonaContext context)
        {
            return OnInvoke(target, context, new TInvokeState());
        }


        public Type ReturnType
        {
            get { return Method.ReturnType; }
        }
    }
}