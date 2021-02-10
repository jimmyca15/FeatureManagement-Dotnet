using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Consoto.Banking.AccountService
{
    public static class EcsFeatureManagementExtensions
    {
        public static IServiceCollection AddEcsFeatureManagement(this IServiceCollection services)
        {
            services.AddSingleton<EcsVariantProvider>();

            services.AddSingleton<IFeatureVariantResolver>(sp => sp.GetRequiredService<EcsVariantProvider>());

            //
            // May want a way to fall back to default variant provider rather than full replacement
            services.AddSingleton<IFeatureDefinitionProvider>(sp => sp.GetRequiredService<EcsVariantProvider>());

            return services;
        }
    }
}
