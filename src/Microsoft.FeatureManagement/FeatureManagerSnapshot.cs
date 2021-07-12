// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.FeatureManagement
{
    /// <summary>
    /// Provides a snapshot of feature state to ensure consistency across a given request.
    /// </summary>
    class FeatureManagerSnapshot : IFeatureManagerSnapshot, IFeatureManagerSnapshot2
    {
        private readonly IFeatureManager2 _featureManager;
        private readonly IDictionary<string, bool> _flagCache = new Dictionary<string, bool>();
        private readonly IDictionary<string, object> _variantCache = new Dictionary<string, object>();
        private IEnumerable<string> _featureNames;

        public FeatureManagerSnapshot(IFeatureManager2 featureManager)
        {
            _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        }

        public async IAsyncEnumerable<string> GetFeatureNamesAsync()
        {
            if (_featureNames == null)
            {
                var featureNames = new List<string>();

                await foreach (string featureName in _featureManager.GetFeatureNamesAsync().ConfigureAwait(false))
                {
                    featureNames.Add(featureName);
                }

                _featureNames = featureNames;
            }

            foreach (string featureName in _featureNames)
            {
                yield return featureName;
            }
        }

        public async ValueTask<T> GetVariantAsync<T>(string feature, CancellationToken cancellationToken = default)
        {
            //
            // First, check local cache
            if (_variantCache.ContainsKey(feature))
            {
                return (T)_variantCache[feature];
            }

            T variant = await _featureManager.GetVariantAsync<T>(feature, cancellationToken).ConfigureAwait(false);

            _variantCache[feature] = variant;

            return variant;
        }

        public async ValueTask<T> GetVariantAsync<T, TContext>(string feature, TContext context, CancellationToken cancellationToken = default)
        {
            //
            // First, check local cache
            if (_variantCache.ContainsKey(feature))
            {
                return (T)_variantCache[feature];
            }

            T variant = await _featureManager.GetVariantAsync<T, TContext>(feature, context, cancellationToken).ConfigureAwait(false);

            _variantCache[feature] = variant;

            return variant;
        }

        public Task<bool> IsEnabledAsync(string feature)
        {
            return IsEnabledAsync(feature, CancellationToken.None).AsTask();
        }

        public Task<bool> IsEnabledAsync<TContext>(string feature, TContext context)
        {
            return IsEnabledAsync(feature, context, CancellationToken.None).AsTask();
        }

        public async ValueTask<bool> IsEnabledAsync(string feature, CancellationToken cancellationToken)
        {
            //
            // First, check local cache
            if (_flagCache.ContainsKey(feature))
            {
                return _flagCache[feature];
            }

            bool enabled = await _featureManager.IsEnabledAsync(feature, cancellationToken).ConfigureAwait(false);

            _flagCache[feature] = enabled;

            return enabled;
        }

        public async ValueTask<bool> IsEnabledAsync<TContext>(string feature, TContext context, CancellationToken cancellationToken)
        {
            //
            // First, check local cache
            if (_flagCache.ContainsKey(feature))
            {
                return _flagCache[feature];
            }

            bool enabled = await _featureManager.IsEnabledAsync(feature, context, cancellationToken).ConfigureAwait(false);

            _flagCache[feature] = enabled;

            return enabled;
        }
    }
}
