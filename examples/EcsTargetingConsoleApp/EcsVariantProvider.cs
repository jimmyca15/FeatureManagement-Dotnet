using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Consoto.Banking.AccountService
{
    class EcsVariantProvider : IFeatureVariantResolver, IFeatureDefinitionProvider
    {
        private readonly MockEcs _mockEcs = new MockEcs();

        public async IAsyncEnumerable<FeatureDefinition> GetAllFeatureDefinitionsAsync()
        {
            await Task.CompletedTask;
            yield break;
        }

        public Task<FeatureDefinition> GetFeatureDefinitionAsync(string featureName)
        {
            return Task.FromResult(new FeatureDefinition
            {
                Name = featureName
            });
        }

        public ValueTask<T> ResolveVariant<T>(FeatureDefinition _, ITargetingContext targetingContext)
        {
            Dictionary<string, string> settings = _mockEcs.GetSettings(new Dictionary<string, string>
            {
                {  MockEcs.UserIdKey, targetingContext.UserId }
            });

            return new ValueTask<T>(new ConfigurationBuilder().AddInMemoryCollection(settings).Build().Get<T>());
        }
    }
}
