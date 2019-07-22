// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Extensions.Configuration;

namespace Microsoft.FeatureManagement
{
    /// <summary>
    /// Used to evaluate whether a feature is enabled or disabled.
    /// </summary>
    public interface IFeatureManager
    {
        /// <summary>
        /// Checks whether a given feature is enabled.
        /// </summary>
        /// <param name="feature">The name of the feature to check.</param>
        /// <returns>True if the feature is enabled, otherwise false.</returns>
        bool IsEnabled(string feature);

        /// <summary>
        /// Gets the configuration that has been assigned for a given feature.
        /// </summary>
        /// <param name="feature">The name of the feature to retrieve configuration for.</param>
        /// <returns>The configuration of the feature.</returns>
        IConfiguration GetConfiguration(string feature);
    }
}
