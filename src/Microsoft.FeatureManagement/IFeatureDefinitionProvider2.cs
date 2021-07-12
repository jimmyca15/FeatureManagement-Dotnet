﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.FeatureManagement
{
    /// <summary>
    /// A provider of feature definitions.
    /// </summary>
    public interface IFeatureDefinitionProvider2 : IFeatureDefinitionProvider
    {
        /// <summary>
        /// Retrieves the definition for a given feature.
        /// </summary>
        /// <param name="featureName">The name of the feature to retrieve the definition for.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>The feature's definition.</returns>	
        Task<FeatureDefinition> GetFeatureDefinitionAsync(string featureName, CancellationToken cancellationToken);
    }
}