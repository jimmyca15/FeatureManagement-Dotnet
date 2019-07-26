using System.Collections.Generic;

namespace Microsoft.FeatureManagement
{
    public class FeatureAssignmentContext
    {
        public string FeatureName { get; set; }

        public IEnumerable<AssignmentChoice> AssignmentChoices { get; set; }
    }
}
