using System.Threading.Tasks;

namespace Microsoft.FeatureManagement
{
    /// <summary>
    /// Provides a method to assign a variant of a feature to be used based off of custom conditions.
    /// </summary>
    public interface IFeatureVariantAssigner : IFeatureVariantAssignerMetadata
    {
        /// <summary>
        /// Assign a variant of a feature to be used based off of customized criteria.
        /// </summary>
        /// <param name="variantAssignmentContext">Information provided by the system to be used during the assignment process.</param>
        /// <returns>The variant that should be assigned for a given feature.</returns>
        ValueTask<FeatureVariant> AssignVariantAsync(FeatureVariantAssignmentContext variantAssignmentContext);
    }
}
