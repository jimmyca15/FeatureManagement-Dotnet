using Microsoft.FeatureManagement.FeatureFilters;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.FeatureManagement.Targeting
{
    class TargetingEvaluator
    {
        private static StringComparison ComparisonType(bool ignoreCase) => ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        public static bool IsTargeted(Audience audience, ITargetingContext targetingContext, bool ignoreCase, string hint)
        {
            if (audience == null)
            {
                throw new ArgumentNullException(nameof(audience));
            }

            if (targetingContext == null)
            {
                throw new ArgumentNullException(nameof(targetingContext));
            }

            ValidateAudience(audience);


            //
            // Check if the user is being targeted directly
            if (targetingContext.UserId != null &&
                audience.Users != null &&
                audience.Users.Any(user => targetingContext.UserId.Equals(user, ComparisonType(ignoreCase))))
            {
                return true;
            }

            //
            // Check if the user is in a group that is being targeted
            if (targetingContext.Groups != null &&
                audience.Groups != null)
            {
                foreach (string group in targetingContext.Groups)
                {
                    GroupRollout groupRollout = audience.Groups.FirstOrDefault(g => g.Name.Equals(group, ComparisonType(ignoreCase)));

                    if (groupRollout != null)
                    {
                        string audienceContextId = $"{targetingContext.UserId}\n{hint}\n{group}";

                        if (IsTargeted(audienceContextId, groupRollout.RolloutPercentage))
                        {
                            return true;
                        }
                    }
                }
            }

            //
            // Check if the user is being targeted by a default rollout percentage
            string defaultContextId = $"{targetingContext.UserId}\n{hint}";

            return IsTargeted(defaultContextId, audience.DefaultRolloutPercentage);
        }


        /// <summary>
        /// Determines if a given context id should be targeted based off the provided percentage
        /// </summary>
        /// <param name="contextId">A context identifier that determines what the percentage is applicable for</param>
        /// <param name="percentage">The total percentage of possible context identifiers that should be targeted</param>
        /// <returns>A boolean representing if the context identifier should be targeted</returns>
        private static bool IsTargeted(string contextId, double percentage)
        {
            byte[] hash;

            using (HashAlgorithm hashAlgorithm = SHA256.Create())
            {
                hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(contextId));
            }

            //
            // Use first 4 bytes for percentage calculation
            // Cryptographic hashing algorithms ensure adequate entropy across hash
            uint contextMarker = BitConverter.ToUInt32(hash, 0);

            double contextPercentage = (contextMarker / (double)uint.MaxValue) * 100;

            return contextPercentage < percentage;
        }

        private static bool ValidateAudience(Audience audience)
        {
            const string OutOfRange = "The value is out of the accepted range.";

            if (audience == null)
            {
                throw new ArgumentNullException(nameof(audience));
            }

            if (audience.DefaultRolloutPercentage < 0 || audience.DefaultRolloutPercentage > 100)
            {
                string paramName = $"{audience}.{audience.DefaultRolloutPercentage}";

                throw new ArgumentException(OutOfRange, paramName);
            }

            if (audience.Groups != null)
            {
                int index = 0;

                foreach (GroupRollout groupRollout in audience.Groups)
                {
                    index++;

                    if (groupRollout.RolloutPercentage < 0 || groupRollout.RolloutPercentage > 100)
                    {
                        //
                        // Audience.Groups[1].RolloutPercentage
                        string paramName = $"{audience}.{audience.Groups}[{index}].{groupRollout.RolloutPercentage}";

                        throw new ArgumentException(OutOfRange, paramName);
                    }
                }
            }

            return true;
        }
    }
}
