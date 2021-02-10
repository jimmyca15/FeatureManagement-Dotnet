using Microsoft.FeatureManagement.FeatureFilters;
using System.Threading.Tasks;

namespace Microsoft.FeatureManagement
{
    public interface IFeatureVariantResolver
    {
        ValueTask<T> ResolveVariant<T>(FeatureDefinition featureDefinition, ITargetingContext targetingContext);
    }
}
