﻿using System.Threading.Tasks;

namespace Microsoft.FeatureManagement
{
    /// <summary>
    /// Provides a method to assign a variant of a feature to be used based off of custom conditions.
    /// </summary>
    /// <typeparam name="TContext">A custom type that the assigner requires to perform assignment</typeparam>
    public interface IContextualFeatureVariantAssigner<TContext> : IFeatureVariantAssignerMetadata
    {
        /// <summary>
        /// Assign a variant of a feature to be used based off of customized criteria.
        /// </summary>
        /// <param name="variantAssignmentContext">Information provided by the system to be used during the assignment process.</param>
        /// <param name="appContext">A context defined by the application that is passed in to the feature management system to provide contextual information for assigning a variant of a feature.</param>
        /// <returns>The variant that should be assigned for a given feature.</returns>
        ValueTask<FeatureVariant> AssignVariantAsync(FeatureVariantAssignmentContext variantAssignmentContext, TContext appContext);
    }
}
