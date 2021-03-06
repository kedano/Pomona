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

using Nancy;
using Nancy.Routing;

using Pomona.Common;

namespace Pomona.Routing
{
    /// <summary>
    /// The default route metadata provider for Pomona.
    /// </summary>
    internal class PomonaRouteMetadataProvider : IRouteMetadataProvider
    {
        public const string ClientAssembly = Prefix + "ClientAssembly";
        public const string ClientNugetPackage = Prefix + "ClientNugetPackage";
        public const string ClientNugetPackageVersioned = Prefix + "ClientNugetPackageVersioned";
        public const string JsonSchema = Prefix + "JsonSchema";
        public const string Prefix = "Pomona.Metadata.";


        /// <summary>
        /// Gets the metadata for the provided route.
        /// </summary>
        /// <param name="module">The <see cref="T:Nancy.INancyModule" /> instance that the route is declared in.</param>
        /// <param name="routeDescription">A <see cref="T:Nancy.Routing.RouteDescription" /> for the route.</param>
        /// <returns>
        /// An object representing the metadata for the given route, or <see langword="null" /> if nothing is found.
        /// </returns>
        public object GetMetadata(INancyModule module, RouteDescription routeDescription)
        {
            if (String.IsNullOrWhiteSpace(routeDescription.Name))
                return null;

            // TODO: Yikes, what an ugly hack. We need to figure out a better way to identify routes than their name. [asbjornu]
            switch (routeDescription.Name)
            {
                case JsonSchema:
                    return new PomonaRouteMetadata
                    {
                        ContentType = "application/json",
                        Method = HttpMethod.Get,
                        Relation = "json-schema",
                    };

                case ClientAssembly:
                    return new PomonaRouteMetadata
                    {
                        ContentType = "binary/octet-stream",
                        Method = HttpMethod.Get,
                        Relation = "client-assembly",
                    };

                case ClientNugetPackage:
                case ClientNugetPackageVersioned:
                    return new PomonaRouteMetadata
                    {
                        ContentType = "application/zip",
                        Method = HttpMethod.Get,
                        Relation = "nuget-package",
                    };
            }

            return null;
        }


        /// <summary>
        /// Gets the <see cref="T:System.Type" /> of the metadata that is created by the provider.
        /// </summary>
        /// <param name="module">The <see cref="T:Nancy.INancyModule" /> instance that the route is declared in.</param>
        /// <param name="routeDescription">A <see cref="T:Nancy.Routing.RouteDescription" /> for the route.</param>
        /// <returns>
        /// A <see cref="T:System.Type" /> instance, or <see langword="null" /> if nothing is found.
        /// </returns>
        public Type GetMetadataType(INancyModule module, RouteDescription routeDescription)
        {
            if (String.IsNullOrWhiteSpace(routeDescription.Name))
                return null;

            // TODO: Yikes, what an ugly hack. We need to figure out a better way to identify routes than their name. [asbjornu]
            switch (routeDescription.Name)
            {
                case JsonSchema:
                case ClientAssembly:
                case ClientNugetPackage:
                case ClientNugetPackageVersioned:
                    return typeof(PomonaRouteMetadata);
            }

            return null;
        }
    }
}