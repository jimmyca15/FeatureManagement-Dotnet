﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Microsoft.FeatureManagement
{
    /// <summary>
    /// The configuration of a feature filter.
    /// </summary>
    public class FeatureFilterConfiguration
    {
        /// <summary>
        /// A feature filter configuration that results in the feature always being enabled.
        /// </summary>
        public static FeatureFilterConfiguration AlwaysOn = new FeatureFilterConfiguration
        {
            Name = "AlwaysOn"
        };

        /// <summary>
        /// The name of the feature filter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Configurable parameters that can change across instances of a feature filter.
        /// </summary>
        public IConfiguration Parameters { get; set; } = new ConfigurationRoot(new List<IConfigurationProvider>());
    }
}
