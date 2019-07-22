// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.FeatureManagement
{
    /// <summary>
    /// A feature settings provider that pulls settings from the .NET Core <see cref="IConfiguration"/> system.
    /// </summary>
    sealed class ConfigurationFeatureSettingsProvider : IFeatureSettingsProvider
    {
        private const string FeatureFiltersSectionName = "EnabledFor";
        private readonly IConfiguration _configuration;

        public ConfigurationFeatureSettingsProvider(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IAssignerSettings TryGetAssignerSettings(string assignerName)
        {
            IConfigurationSection configuration = GetFeatureManagementSection("Assigners");

            IEnumerable<IConfigurationSection> assignerSections = configuration.GetChildren();

            AssignerSettings settings = new AssignerSettings
            {
                Name = assignerName
            };

            List<AssignmentChoice> assignments = new List<AssignmentChoice>();

            foreach (IConfigurationSection section in assignerSections)
            {
                //
                // Arrays in json such as "myKey": [ "some", "values" ]
                // Are accessed through the configuration system by using the array index as the property name, e.g. "myKey": { "0": "some", "1": "values" }
                if (int.TryParse(section.Key, out int _) && !string.IsNullOrEmpty(section["Name"]) && assignerName.Equals(section["Name"], StringComparison.OrdinalIgnoreCase))
                {
                    foreach (IConfigurationSection subSection in section.GetSection("Assignments").GetChildren())
                    {
                        if (int.TryParse(subSection.Key, out int __) && !string.IsNullOrEmpty(subSection["Name"]))
                        {
                            assignments.Add(new AssignmentChoice
                            {
                                Name = subSection["Name"],
                                Parameters = subSection.GetSection("Parameters")
                            });
                        }
                    }
                }
            }

            settings.Assignments = assignments;

            return settings;
        }

        public IFeatureSettings TryGetFeatureSettings(string featureName)
        {
            /*
              
            We support
            
            myFeature: {
              enabledFor: [ "myFeatureFilter1", "myFeatureFilter2" ]
            },
            myDisabledFeature: {
              enabledFor: [  ]
            },
            myFeature2: {
              enabledFor: "myFeatureFilter1;myFeatureFilter2"
            },
            myDisabledFeature2: {
              enabledFor: ""
            },
            myFeature3: "myFeatureFilter1;myFeatureFilter2",
            myDisabledFeature3: "",
            myAlwaysEnabledFeature: true,
            myAlwaysDisabledFeature: false // removing this line would be the same as setting it to false
            myAlwaysEnabledFeature2: {
              enabledFor: true
            },
            myAlwaysDisabledFeature2: {
              enabledFor: false
            }

            */

            IConfigurationSection configuration = GetFeatureManagementSection(featureName);

            var enabledFor = new List<FeatureFilterSettings>();

            string val = configuration.Value; // configuration[$"{featureName}"];

            if (string.IsNullOrEmpty(val))
            {
                val = configuration[FeatureFiltersSectionName];
            }

            if (!string.IsNullOrEmpty(val) && bool.TryParse(val, out bool result) && result)
            {
                //
                //myAlwaysEnabledFeature: true
                // OR
                //myAlwaysEnabledFeature: {
                //  enabledFor: true
                //}
                enabledFor.Add(new FeatureFilterSettings
                {
                    Name = "AlwaysOn"
                });
            }
            else
            {
                IEnumerable<IConfigurationSection> filterSections = configuration.GetSection(FeatureFiltersSectionName).GetChildren();

                foreach (IConfigurationSection section in filterSections)
                {
                    //
                    // Arrays in json such as "myKey": [ "some", "values" ]
                    // Are accessed through the configuration system by using the array index as the property name, e.g. "myKey": { "0": "some", "1": "values" }
                    if (int.TryParse(section.Key, out int i) && !string.IsNullOrEmpty(section[nameof(FeatureFilterSettings.Name)]))
                    {
                        enabledFor.Add(new FeatureFilterSettings()
                        {
                            Name = section[nameof(FeatureFilterSettings.Name)],
                            Parameters = section.GetSection(nameof(FeatureFilterSettings.Parameters))
                        });
                    }
                }
            }

            List<FeatureVariant> variants = new List<FeatureVariant>();

            foreach (IConfigurationSection section in configuration.GetSection("Variants").GetChildren())
            {
                if (int.TryParse(section.Key, out int i) && !string.IsNullOrEmpty(section[nameof(FeatureFilterSettings.Name)]))
                {
                    variants.Add(new FeatureVariant
                    {
                        Name = section[nameof(FeatureFilterSettings.Name)],
                        TrackingId = null,
                        Assignments = section.GetSection("Assignments").Get<List<string>>() ?? new List<string>(),
                        Configuration = section.GetSection("Configuration")
                    });
                }
            }

            return new FeatureSettings()
            {
                Name = featureName,
                EnabledFor = enabledFor,
                Variants = variants
            };
        }

        private IConfigurationSection GetFeatureManagementSection(string sectionName)
        {
            const string FeatureManagementSectionName = "FeatureManagement";

            //
            // Look for settings under the "FeatureManagement" section
            IConfigurationSection featureConfiguration = _configuration.GetSection(FeatureManagementSectionName).GetChildren().FirstOrDefault(section => section.Key.Equals(sectionName, StringComparison.OrdinalIgnoreCase));

            //
            // Fallback to the configuration section using the feature's name
            if (featureConfiguration == null)
            {
                featureConfiguration = _configuration.GetSection(sectionName);
            }

            return featureConfiguration;
        }
    }
}
