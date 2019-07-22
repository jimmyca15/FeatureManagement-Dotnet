using System.Collections.Generic;

namespace Microsoft.FeatureManagement
{
    public class FeatureAssignmentContext
    {
        public IEnumerable<AssignmentChoice> AssignmentChoices { get; set; }
    }
}
