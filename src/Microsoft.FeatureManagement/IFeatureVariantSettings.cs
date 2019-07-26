using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Microsoft.FeatureManagement
{
    interface IFeatureVariantSettings
    {
        string Name { get; set; }

        string TrackingId { get; set; }

        IEnumerable<string> Assignments { get; }

        IConfiguration Configuration { get; }
    }
}
