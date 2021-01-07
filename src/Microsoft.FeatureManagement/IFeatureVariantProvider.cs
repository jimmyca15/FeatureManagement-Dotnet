using System.Threading.Tasks;

namespace Microsoft.FeatureManagement
{
    interface IFeatureVariantProvider
    {
        ValueTask<T> GetVariant<T>(FeatureDefinition featureDefinition, FeatureVariant variant);
    }
}
