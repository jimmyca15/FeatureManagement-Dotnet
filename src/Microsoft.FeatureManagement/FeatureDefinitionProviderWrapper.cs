using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.FeatureManagement
{
    sealed class FeatureDefinitionProviderWrapper : IFeatureDefinitionProvider2
    {
        private readonly IFeatureDefinitionProvider _provider;

        public FeatureDefinitionProviderWrapper(IFeatureDefinitionProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            _provider = provider;
        }

        public Task<FeatureDefinition> GetFeatureDefinitionAsync(string featureName, CancellationToken cancellationToken)
        {
            if (_provider is IFeatureDefinitionProvider2 provider2)
            {
                return provider2.GetFeatureDefinitionAsync(featureName, cancellationToken);
            }

            return _provider.GetFeatureDefinitionAsync(featureName);
        }

        public IAsyncEnumerable<FeatureDefinition> GetAllFeatureDefinitionsAsync()
        {
            return _provider.GetAllFeatureDefinitionsAsync();
        }

        public Task<FeatureDefinition> GetFeatureDefinitionAsync(string featureName)
        {
            return _provider.GetFeatureDefinitionAsync(featureName);
        }
    }
}
