// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement.FeatureFilters;
using Microsoft.FeatureManagement.Utils;
using System.Collections.Generic;

namespace Microsoft.FeatureManagement.FeatureAssigners
{
    public class PercentageAssigner : IFeatureAssigner
    {
        public string Assign(FeatureAssignmentContext context)
        {
            Dictionary<string, PercentageSettings> map = new Dictionary<string, PercentageSettings>();

            foreach (var choice in context.AssignmentChoices)
            {
                map[choice.Name] = choice.Parameters.Get<PercentageSettings>();
            }

            string assignment = null;

            int random = (int) (RandomGenerator.NextDouble() * 100);

            foreach (KeyValuePair<string, PercentageSettings> choice in map)
            {
                random -= choice.Value?.Value ?? 0;

                if (random < 0)
                {
                    assignment = choice.Key;

                    break;
                }
            }

            return assignment;
        }
    }
}
