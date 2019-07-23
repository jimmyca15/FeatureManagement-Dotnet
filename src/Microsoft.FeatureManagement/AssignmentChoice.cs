using Microsoft.Extensions.Configuration;

namespace Microsoft.FeatureManagement
{
    /// <summary>
    /// A possible choice that an assigner can choose between when making an assignment during feature evaluation.
    /// </summary>
    public class AssignmentChoice
    {
        /// <summary>
        /// The name of the assignment that can be made.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Parameters for the assigner to utilize when deciding which assignment to make.
        /// </summary>
        public IConfiguration Parameters { get; set; }
    }
}
