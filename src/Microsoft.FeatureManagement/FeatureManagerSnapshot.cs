// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Microsoft.FeatureManagement
{
    /// <summary>
    /// Provides a snapshot of feature state to ensure consistency across a given request.
    /// </summary>
    class FeatureManagerSnapshot : IFeatureManagerSnapshot
    {
        private readonly IFeatureManager _featureManager;
        private readonly IDictionary<string, bool> _flagCache = new Dictionary<string, bool>();
        private readonly IDictionary<string, IConfiguration> _configurationCache = new Dictionary<string, IConfiguration>();

        public FeatureManagerSnapshot(IFeatureManager featureManager)
        {
            _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        }

        public IConfiguration GetConfiguration(string feature)
        {
            //
            // First, check local cache
            if (_configurationCache.ContainsKey(feature))
            {
                return _configurationCache[feature];
            }

            IConfiguration configuration = _featureManager.GetConfiguration(feature);

            _configurationCache[feature] = configuration;

            return configuration;
        }

        public bool IsEnabled(string feature)
        {
            //
            // First, check local cache
            if (_flagCache.ContainsKey(feature))
            {
                return _flagCache[feature];
            }

            bool enabled = _featureManager.IsEnabled(feature);

            _flagCache[feature] = enabled;

            return enabled;
        }
    }
}
