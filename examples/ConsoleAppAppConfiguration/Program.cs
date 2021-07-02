// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using System;
using System.Threading.Tasks;

namespace ConsoleAppAppConfiguration
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            IConfigurationRefresher refresher = null;

            //
            // Setup configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddAzureAppConfiguration(o => 
                {
                    o.Connect(Environment.GetEnvironmentVariable("AppConfigurationConnectionString"));

                    //
                    // Feature flags have a 30 second cache period by default
                    // Refresh is a no-op during that period
                    o.UseFeatureFlags();

                    refresher = o.GetRefresher();
                })
                .Build();

            //
            // Setup application services + feature management
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton(configuration)
                    .AddFeatureManagement();

            //
            // Get the feature manager from application services
            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                IFeatureManager featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

                string featureName = "Beta";

                while (true)
                {
                    Console.WriteLine($"{featureName} is {(await featureManager.IsEnabledAsync(featureName) ? "enabled" : "disabled")}");

                    await Task.Delay(TimeSpan.FromSeconds(3));

                    await refresher.RefreshAsync();
                }
            }
        }
    }
}
