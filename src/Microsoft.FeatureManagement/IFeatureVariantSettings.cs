using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Microsoft.FeatureManagement
{
    interface IFeatureVariantSettings
    {
        string Name { get; set; }

        string TrackingId { get; set; }

        bool Default { get; set; }

        TargetingSettings Targeting { get; set; }

        IEnumerable<string> Assignments { get; }

        IConfiguration Configuration { get; }
    }

    class TargetingSettings
    {
        public List<string> Users { get; set; } = new List<string>();

        public List<AudienceRolloutSettings> AudienceRollouts { get; set; } = new List<AudienceRolloutSettings>();

        public DefaultRolloutSettings DefaultRollout { get; set; } = new DefaultRolloutSettings();
    }

    class AudienceRolloutSettings
    {
        public string Audience { get; set; }

        public int Percentage { get; set; }
    }

    class DefaultRolloutSettings
    {
        public int Percentage { get; set; }
    }
}
