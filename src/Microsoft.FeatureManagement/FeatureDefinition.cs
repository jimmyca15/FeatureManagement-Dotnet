// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System.Collections.Generic;

namespace Microsoft.FeatureManagement
{
    /// <summary>
    /// The definition of a feature.
    /// </summary>
    public class FeatureDefinition
    {
        /// <summary>
        /// The name of the feature.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The feature filters that the feature can be enabled for.
        /// </summary>
        public IEnumerable<FeatureFilterConfiguration> EnabledFor { get; set; } = new List<FeatureFilterConfiguration>();

        /// <summary>
        /// The assigner used to pick the variant that should be used when a variant is requested
        /// </summary>
        public string Assigner { get; set; }

        /// <summary>
        /// The feature variants listed for this feature.
        /// </summary>
        public IEnumerable<FeatureVariant> Variants { get; set; } = new List<FeatureVariant>();
    }
}
