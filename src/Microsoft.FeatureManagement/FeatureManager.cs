// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.FeatureManagement.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.FeatureManagement
{
    /// <summary>
    /// Used to evaluate whether a feature is enabled or disabled.
    /// </summary>
    class FeatureManager : IFeatureManager
    {
        private readonly IFeatureSettingsProvider _settingsProvider;
        private readonly IEnumerable<IFeatureFilter> _featureFilters;
        private readonly IEnumerable<ISessionManager> _sessionManagers;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, IFeatureFilter> _filterCache;
        private readonly IUserContext _userAccessor;

        public FeatureManager(IFeatureSettingsProvider settingsProvider, IEnumerable<IFeatureFilter> featureFilters, IEnumerable<ISessionManager> sessionManagers, ILoggerFactory loggerFactory, IUserContext userAccessor)
        {
            _settingsProvider = settingsProvider;
            _featureFilters = featureFilters ?? throw new ArgumentNullException(nameof(featureFilters));
            _sessionManagers = sessionManagers ?? throw new ArgumentNullException(nameof(sessionManagers));
            _logger = loggerFactory.CreateLogger<FeatureManager>();
            _filterCache = new ConcurrentDictionary<string, IFeatureFilter>();
            _userAccessor = userAccessor ?? throw new ArgumentNullException(nameof(userAccessor));
        }

        public IConfiguration GetConfiguration(string feature)
        {
            IFeatureSettings settings = _settingsProvider.TryGetFeatureSettings(feature);

            IConfiguration result = null;

            string rolloutId = null;

            string username = _userAccessor.UserId;

            //
            // Check user assignment
            if (!string.IsNullOrEmpty(username))
            {
                foreach (IFeatureVariantSettings variant in settings.Variants)
                {
                    if (variant.Targeting?.Users?.Contains(username, StringComparer.OrdinalIgnoreCase) ?? false)
                    {
                        result = variant.Configuration;

                        break;
                    }
                }
            }

            //
            // Check audience assignment
            if (result == null)
            {
                foreach (IFeatureVariantSettings variant in settings.Variants)
                {
                    Dictionary<string, int> bucketAssignments = new Dictionary<string, int>();

                    if (variant.Targeting?.AudienceRollouts == null)
                    {
                        continue;
                    }

                    foreach (AudienceRolloutSettings audience in variant.Targeting.AudienceRollouts)
                    {
                        if (!_userAccessor.Audiences.Contains(audience.Audience, StringComparer.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (!bucketAssignments.TryGetValue(audience.Audience, out int bucketAssignment))
                        {
                            string key = (rolloutId ?? feature) + "\n" + audience.Audience + "\n" + username;

                            uint hash = unchecked((uint)key.GetStableHashCode());

                            double res = ((double) hash / ((double) uint.MaxValue)) * (double) 100;

                            bucketAssignment = (int)res;
                        }

                        bucketAssignment -= audience.Percentage;

                        if (bucketAssignment < 0)
                        {
                            result = variant.Configuration;

                            break;
                        }

                        bucketAssignments[audience.Audience] = bucketAssignment;
                    }

                    if (result != null)
                    {
                        break;
                    }
                }
            }

            //
            // Check user rollout
            if (result == null && !string.IsNullOrEmpty(username))
            {
                string key = (rolloutId ?? feature) + "\n" + username;

                uint hash = unchecked((uint)key.GetStableHashCode());

                double res = ((double)hash / ((double)uint.MaxValue)) * (double)100;

                int bucketAssignment = (int)res;

                foreach (IFeatureVariantSettings variant in settings.Variants)
                {
                    bucketAssignment -= variant.Targeting?.DefaultRollout?.Percentage ?? 0;

                    if (bucketAssignment < 0)
                    {
                        result = variant.Configuration;

                        break;
                    }
                }
            }

            //
            // Assign default
            if (result == null)
            {
                result = settings.Variants.FirstOrDefault(variant => variant.Default ||
                                                                     variant.Name.Equals("Default", StringComparison.OrdinalIgnoreCase))?.Configuration;
            }

            return result ?? new ConfigurationRoot(new List<IConfigurationProvider>());
        }

        public bool IsEnabled(string feature)
        {
            foreach (ISessionManager sessionManager in _sessionManagers)
            {
                if (sessionManager.TryGet(feature, out bool cachedEnabled))
                {
                    return cachedEnabled;
                }
            }

            bool enabled = false;

            IFeatureSettings settings = _settingsProvider.TryGetFeatureSettings(feature);

            if (settings != null)
            {
                //
                // Check if feature is always on
                // If it is, result is true, goto: cache

                if (settings.EnabledFor.Any(featureFilter => string.Equals(featureFilter.Name, "AlwaysOn", StringComparison.OrdinalIgnoreCase)))
                {
                    enabled = true;
                }
                else
                {
                    //
                    // For all enabling filters listed in the feature's state calculate if they return true
                    // If any executed filters return true, return true

                    foreach (IFeatureFilterSettings featureFilterSettings in settings.EnabledFor)
                    {
                        IFeatureFilter filter = GetFeatureFilter(featureFilterSettings.Name);

                        if (filter == null)
                        {
                            _logger.LogWarning($"Feature filter '{featureFilterSettings.Name}' specified for feature '{feature}' was not found.");

                            continue;
                        }

                        var context = new FeatureFilterEvaluationContext()
                        {
                            FeatureName = feature,
                            Parameters = featureFilterSettings.Parameters 
                        };

                        if (filter.Evaluate(context))
                        {
                            enabled = true;

                            break;
                        }
                    }
                }
            }

            foreach (ISessionManager sessionManager in _sessionManagers)
            {
                sessionManager.Set(feature, enabled);
            }

            return enabled;
        }

        private IFeatureFilter GetFeatureFilter(string filterName)
        {
            const string filterSuffix = "filter";

            IFeatureFilter filter = _filterCache.GetOrAdd(
                filterName,
                (_) => {

                    IEnumerable<IFeatureFilter> matchingFilters = _featureFilters.Where(f =>
                    {
                        Type t = f.GetType();

                        string name = ((FilterAliasAttribute)Attribute.GetCustomAttribute(t, typeof(FilterAliasAttribute)))?.Alias;

                        if (name == null)
                        {
                            name = t.Name.EndsWith(filterSuffix, StringComparison.OrdinalIgnoreCase) ? t.Name.Substring(0, t.Name.Length - filterSuffix.Length) : t.Name;
                        }

                        //
                        // Feature filters can have namespaces in their alias
                        // If a feature is configured to use a filter without a namespace such as 'MyFilter', then it can match 'MyOrg.MyProduct.MyFilter' or simply 'MyFilter'
                        // If a feature is configured to use a filter with a namespace such as 'MyOrg.MyProduct.MyFilter' then it can only match 'MyOrg.MyProduct.MyFilter' 
                        if (filterName.Contains('.'))
                        {
                            //
                            // The configured filter name is namespaced. It must be an exact match.
                            return string.Equals(name, filterName, StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            //
                            // We take the simple name of a filter, E.g. 'MyFilter' for 'MyOrg.MyProduct.MyFilter'
                            string simpleName = name.Contains('.') ? name.Split('.').Last() : name;

                            return string.Equals(simpleName, filterName, StringComparison.OrdinalIgnoreCase);
                        }
                    });

                    if (matchingFilters.Count() > 1)
                    {
                        throw new InvalidOperationException($"Multiple feature filters match the configured filter named '{filterName}'.");
                    }

                    return matchingFilters.FirstOrDefault();
                }
            );

            return filter;
        }
    }
}
