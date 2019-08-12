using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Microsoft.FeatureManagement
{
    class FeatureVariantSettings : IFeatureVariantSettings
    {
        public string Name { get; set; }

        public string TrackingId { get; set; }

        public bool Default { get; set; }

        public TargetingSettings Targeting { get; set; }

        public IEnumerable<string> Assignments { get; set; } = new List<string>();

        public IConfiguration Configuration { get; set; }
    }
}
