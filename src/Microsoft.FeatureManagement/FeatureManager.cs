﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.FeatureFilters;
using Microsoft.FeatureManagement.Targeting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.FeatureManagement
{
    /// <summary>
    /// Used to evaluate whether a feature is enabled or disabled.
    /// </summary>
    class FeatureManager : IFeatureManager
    {
        private readonly IFeatureDefinitionProvider _featureDefinitionProvider;
        private readonly IFeatureVariantProvider _featureVariantProvider;
        private readonly IEnumerable<IFeatureFilterMetadata> _featureFilters;
        private readonly IEnumerable<ISessionManager> _sessionManagers;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, IFeatureFilterMetadata> _filterMetadataCache;
        private readonly ConcurrentDictionary<string, ContextualFeatureFilterEvaluator> _contextualFeatureFilterCache;
        private readonly FeatureManagementOptions _options;

        public FeatureManager(
            IFeatureDefinitionProvider featureDefinitionProvider,
            IFeatureVariantProvider featureVariantProvider,
            IEnumerable<IFeatureFilterMetadata> featureFilters,
            IEnumerable<ISessionManager> sessionManagers,
            ILoggerFactory loggerFactory,
            IOptions<FeatureManagementOptions> options)
        {
            _featureDefinitionProvider = featureDefinitionProvider;
            _featureVariantProvider = featureVariantProvider;
            _featureFilters = featureFilters ?? throw new ArgumentNullException(nameof(featureFilters));
            _sessionManagers = sessionManagers ?? throw new ArgumentNullException(nameof(sessionManagers));
            _logger = loggerFactory.CreateLogger<FeatureManager>();
            _filterMetadataCache = new ConcurrentDictionary<string, IFeatureFilterMetadata>();
            _contextualFeatureFilterCache = new ConcurrentDictionary<string, ContextualFeatureFilterEvaluator>();
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public Task<bool> IsEnabledAsync(string feature)
        {
            return IsEnabledAsync<object>(feature, null, false);
        }

        public Task<bool> IsEnabledAsync<TContext>(string feature, TContext appContext)
        {
            return IsEnabledAsync(feature, appContext, true);
        }

        public async IAsyncEnumerable<string> GetFeatureNamesAsync()
        {
            await foreach (FeatureDefinition featureDefintion in _featureDefinitionProvider.GetAllFeatureDefinitionsAsync().ConfigureAwait(false))
            {
                yield return featureDefintion.Name;
            }
        }

        public async ValueTask<T> GetVariantAsync<T, TContext>(string feature, TContext targetingContext) where TContext : ITargetingContext
        {
            if (feature == null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            if (targetingContext == null)
            {
                throw new ArgumentNullException(nameof(targetingContext));
            }

            FeatureDefinition featureDefinition = await _featureDefinitionProvider.GetFeatureDefinitionAsync(feature).ConfigureAwait(false);

            if (featureDefinition == null)
            {
                return default(T);
            }

            FeatureVariant variant = null;

            FeatureVariant defaultVariant = null;

            double cumulativePercentage = 0;

            var cumulativeGroups = new Dictionary<string, double>();

            if (featureDefinition.Variants != null)
            {
                foreach (FeatureVariant v in featureDefinition.Variants)
                {
                    if (defaultVariant == null && v.Default)
                    {
                        defaultVariant = v;
                    }

                    Audience audience = AccumulateAudience(v.Audience, ref cumulativePercentage, ref cumulativeGroups);

                    if (TargetingEvaluator.IsTargeted(audience, targetingContext, true, feature))
                    {
                        variant = v;

                        break;
                    }
                }
            }

            if (variant == null)
            {
                variant = defaultVariant;
            }

            if (variant == null)
            {
                return default(T);
            }

            return await _featureVariantProvider.GetVariant<T>(featureDefinition, variant);
        }

        public async ValueTask<T> GetVariantAsync<T>(string feature)
        {
            ITargetingContextAccessor targetingContextAccessor = null;

            return await GetVariantAsync<T, TargetingContext>(feature, await targetingContextAccessor.GetContextAsync());
        }

        private static Audience AccumulateAudience(Audience audience, ref double cumulativePercentage, ref Dictionary<string, double> cumulativeGroups)
        {
            Audience ret = new Audience();

            ret.Users = audience.Users;

            ret.Groups = new List<GroupRollout>();

            foreach (GroupRollout gr in audience.Groups)
            {
                double percentage = gr.RolloutPercentage;

                if (cumulativeGroups.TryGetValue(gr.Name, out double p))
                {
                    percentage += p;
                }

                percentage = Math.Min(percentage, 100);

                cumulativeGroups[gr.Name] = percentage;

                ret.Groups.Add(new GroupRollout
                {
                    Name = gr.Name,
                    RolloutPercentage = percentage
                });
            }

            cumulativePercentage = cumulativePercentage + audience.DefaultRolloutPercentage;

            ret.DefaultRolloutPercentage = cumulativePercentage;

            return ret;
        }

        private async Task<bool> IsEnabledAsync<TContext>(string feature, TContext appContext, bool useAppContext)
        {
            foreach (ISessionManager sessionManager in _sessionManagers)
            {
                bool? readSessionResult = await sessionManager.GetAsync(feature).ConfigureAwait(false);

                if (readSessionResult.HasValue)
                {
                    return readSessionResult.Value;
                }
            }

            bool enabled = false;

            FeatureDefinition featureDefinition = await _featureDefinitionProvider.GetFeatureDefinitionAsync(feature).ConfigureAwait(false);

            if (featureDefinition != null)
            {
                //
                // Check if feature is always on
                // If it is, result is true, goto: cache

                if (featureDefinition.EnabledFor.Any(featureFilter => string.Equals(featureFilter.Name, "AlwaysOn", StringComparison.OrdinalIgnoreCase)))
                {
                    enabled = true;
                }
                else
                {
                    //
                    // For all enabling filters listed in the feature's state calculate if they return true
                    // If any executed filters return true, return true

                    foreach (FeatureFilterConfiguration featureFilterConfiguration in featureDefinition.EnabledFor)
                    {
                        IFeatureFilterMetadata filter = GetFeatureFilterMetadata(featureFilterConfiguration.Name);

                        if (filter == null)
                        {
                            string errorMessage = $"The feature filter '{featureFilterConfiguration.Name}' specified for feature '{feature}' was not found.";

                            if (!_options.IgnoreMissingFeatureFilters)
                            {
                                throw new FeatureManagementException(FeatureManagementError.MissingFeatureFilter, errorMessage);
                            }
                            else
                            {
                                _logger.LogWarning(errorMessage);
                            }

                            continue;
                        }

                        var context = new FeatureFilterEvaluationContext()
                        {
                            FeatureName = feature,
                            Parameters = featureFilterConfiguration.Parameters 
                        };

                        //
                        // IContextualFeatureFilter
                        if (useAppContext)
                        {
                            ContextualFeatureFilterEvaluator contextualFilter = GetContextualFeatureFilter(featureFilterConfiguration.Name, typeof(TContext));

                            if (contextualFilter != null && await contextualFilter.EvaluateAsync(context, appContext).ConfigureAwait(false))
                            {
                                enabled = true;

                                break;
                            }
                        }

                        //
                        // IFeatureFilter
                        if (filter is IFeatureFilter featureFilter && await featureFilter.EvaluateAsync(context).ConfigureAwait(false))
                        {
                            enabled = true;

                            break;
                        }
                    }
                }
            }

            foreach (ISessionManager sessionManager in _sessionManagers)
            {
                await sessionManager.SetAsync(feature, enabled).ConfigureAwait(false);
            }

            return enabled;
        }

        private IFeatureFilterMetadata GetFeatureFilterMetadata(string filterName)
        {
            const string filterSuffix = "filter";

            IFeatureFilterMetadata filter = _filterMetadataCache.GetOrAdd(
                filterName,
                (_) => {

                    IEnumerable<IFeatureFilterMetadata> matchingFilters = _featureFilters.Where(f =>
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
                        throw new FeatureManagementException(FeatureManagementError.AmbiguousFeatureFilter, $"Multiple feature filters match the configured filter named '{filterName}'.");
                    }

                    return matchingFilters.FirstOrDefault();
                }
            );

            return filter;
        }

        private ContextualFeatureFilterEvaluator GetContextualFeatureFilter(string filterName, Type appContextType)
        {
            if (appContextType == null)
            {
                throw new ArgumentNullException(nameof(appContextType));
            }

            ContextualFeatureFilterEvaluator filter = _contextualFeatureFilterCache.GetOrAdd(
                $"{filterName}{Environment.NewLine}{appContextType.FullName}",
                (_) => {

                    IFeatureFilterMetadata metadata = GetFeatureFilterMetadata(filterName);

                    return ContextualFeatureFilterEvaluator.IsContextualFilter(metadata, appContextType) ?
                        new ContextualFeatureFilterEvaluator(metadata, appContextType) :
                        null;
                }
            );

            return filter;
        }
    }
}
