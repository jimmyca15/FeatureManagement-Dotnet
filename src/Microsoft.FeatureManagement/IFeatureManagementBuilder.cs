// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.FeatureManagement
{
    /// <summary>
    /// Provides a way to customize feature management functionality.
    /// </summary>
    public interface IFeatureManagementBuilder
    {
        /// <summary>
        /// The application services.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Adds a given feature filter to the list of feature filters that will be available to enable features during runtime.
        /// </summary>
        /// <typeparam name="T">The feature filter type.</typeparam>
        /// <returns>The feature management builder.</returns>
        IFeatureManagementBuilder AddFeatureFilter<T>() where T : IFeatureFilter;

        /// <summary>
        /// Adds an <see cref="ISessionManager"/> to be used for storing feature state in a session.
        /// </summary>
        /// <typeparam name="T">An implementation of <see cref="ISessionManager"/></typeparam>
        /// <returns>The feature management builder.</returns>
        IFeatureManagementBuilder AddSessionManager<T>() where T : ISessionManager;

        /// <summary>
        /// Adds a given feature assigner to the list of feature assigners that will be available to use when selecting between variants of a given feature.
        /// </summary>
        /// <typeparam name="T">The feature assigner type.</typeparam>
        /// <returns>The feature management builder.</returns>
        IFeatureManagementBuilder AddFeatureAssigner<T>() where T : IFeatureAssigner;
    }
}
