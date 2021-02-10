using Microsoft.FeatureManagement.FeatureFilters;
using Microsoft.FeatureManagement.Targeting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.FeatureManagement
{
    class TargetingFeatureVariantResolver : IFeatureVariantResolver
    {
        private readonly IFeatureVariantProvider _featureVariantProvider;

        public TargetingFeatureVariantResolver(IFeatureVariantProvider featureVariantProvider)
        {
            _featureVariantProvider = featureVariantProvider ?? throw new ArgumentNullException(nameof(featureVariantProvider));
        }

        public async ValueTask<T> ResolveVariant<T>(FeatureDefinition featureDefinition, ITargetingContext targetingContext)
        {
            FeatureVariant variant = Assign(featureDefinition, targetingContext);

            if (variant == null)
            {
                return default(T);
            }

            return await _featureVariantProvider.GetVariant<T>(featureDefinition, variant).ConfigureAwait(false);
        }

        private static FeatureVariant Assign(FeatureDefinition featureDefinition, ITargetingContext targetingContext)
        {
            if (featureDefinition == null)
            {
                return null;
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

                    if (TargetingEvaluator.IsTargeted(audience, targetingContext, true, featureDefinition.Name))
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

            return variant;
        }

        private static Audience AccumulateAudience(Audience audience, ref double cumulativePercentage, ref Dictionary<string, double> cumulativeGroups)
        {
            Audience ret = new Audience();

            ret.Users = audience.Users;

            ret.Groups = new List<GroupRollout>();

            if (audience.Groups != null)
            {
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
            }

            cumulativePercentage = cumulativePercentage + audience.DefaultRolloutPercentage;

            ret.DefaultRolloutPercentage = cumulativePercentage;

            return ret;
        }
    }
}
