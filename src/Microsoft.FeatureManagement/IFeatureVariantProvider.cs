using System.Threading.Tasks;

namespace Microsoft.FeatureManagement
{
    public interface IFeatureVariantProvider
    {
        ValueTask<T> GetVariant<T>(FeatureDefinition featureDefinition, FeatureVariant variant);
    }
}
