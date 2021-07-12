﻿using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.FeatureManagement
{
    /// <summary>
    /// Performs the resolution and binding necessary in the feature variant resolution process. 
    /// </summary>
    public interface IFeatureVariantOptionsResolver
    {
        /// <summary>
        /// Retrieves typed options for a given feature definition and chosen variant.
        /// </summary>
        /// <typeparam name="T">The type of the options to return.</typeparam>
        /// <param name="featureDefinition">The definition of the feature that the resolution is being performed for.</param>
        /// <param name="variant">The chosen variant of the feature.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>Typed options for a given feature definition and chosen variant.</returns>
        ValueTask<T> GetOptions<T>(FeatureDefinition featureDefinition, FeatureVariant variant, CancellationToken cancellationToken);
    }
}
