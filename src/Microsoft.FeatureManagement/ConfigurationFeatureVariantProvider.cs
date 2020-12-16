// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Microsoft.FeatureManagement
{
    /// <summary>
    /// A feature definition provider that pulls feature definitions from the .NET Core <see cref="IConfiguration"/> system.
    /// </summary>
    sealed class ConfigurationFeatureVariantProvider : IFeatureVariantProvider
    {
        private readonly IConfiguration _configuration;

        public ConfigurationFeatureVariantProvider(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public ValueTask<T> GetVariant<T>(FeatureDefinition featureDefinition, FeatureVariant variant)
        {
            return new ValueTask<T>(_configuration.GetSection(variant.ConfigurationReference ?? $"{featureDefinition.Name}:{variant.Name}").Get<T>());
        }
    }
}
