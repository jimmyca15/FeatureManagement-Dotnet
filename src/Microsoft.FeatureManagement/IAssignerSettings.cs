using System.Collections.Generic;

namespace Microsoft.FeatureManagement
{
    interface IAssignerSettings
    {
        string Name { get; set; }

        IEnumerable<AssignmentChoice> Assignments { get; set; }
    }
}
