using Microsoft.FeatureManagement.FeatureFilters;

namespace Microsoft.FeatureManagement
{
    public class FeatureVariant
    {
        public string Name { get; set; }

        public bool Default { get; set; }

        public Audience Audience { get; set; }
    }
}
