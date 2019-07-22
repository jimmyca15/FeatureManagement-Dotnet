namespace Microsoft.FeatureManagement
{
    public interface IFeatureAssigner
    {
        /// <summary>
        /// Requests a feature assigner to make an assignment.
        /// </summary>
        /// <returns></returns>
        string Assign(FeatureAssignmentContext context);
    }
}
