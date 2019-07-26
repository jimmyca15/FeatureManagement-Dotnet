using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using System;
using System.Collections.Generic;

namespace FeatureFlagDemo.FeatureManagement
{
    public class UserRolloutAssigner : IFeatureAssigner
    {
        private IHttpContextAccessor _httpContextAccessor;

        public UserRolloutAssigner(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string Assign(FeatureAssignmentContext context)
        {
            Dictionary<string, UserRolloutSettings> map = new Dictionary<string, UserRolloutSettings>();

            foreach (var choice in context.AssignmentChoices)
            {
                map[choice.Name] = choice.Parameters.Get<UserRolloutSettings>();
            }

            string assignment = null;

            int random = Math.Abs((_httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty).GetHashCode() % 100);

            foreach (KeyValuePair<string, UserRolloutSettings> choice in map)
            {
                random -= choice.Value?.Percentage ?? 0;

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
