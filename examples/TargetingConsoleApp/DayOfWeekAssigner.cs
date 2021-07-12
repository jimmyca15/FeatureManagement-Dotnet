using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using System;
using System.Threading.Tasks;

namespace Consoto.Banking.AccountService
{
    class DayOfWeekAssignmentParameters
    {
        public string DayOfWeek { get; set; }
    }

    public class DayOfWeekAssigner : IFeatureVariantAssigner
    {
        public ValueTask<FeatureVariant> AssignVariantAsync(FeatureVariantAssignmentContext variantAssignmentContext)
        {
            FeatureDefinition featureDefinition = variantAssignmentContext.FeatureDefinition;

            FeatureVariant chosenVariant = null;

            string currentDay = DateTimeOffset.UtcNow.DayOfWeek.ToString();

            foreach (var variant in featureDefinition.Variants)
            {
                DayOfWeekAssignmentParameters p = variant.AssignmentParameters.Get<DayOfWeekAssignmentParameters>();

                if (p.DayOfWeek.Equals(currentDay, StringComparison.OrdinalIgnoreCase))
                {
                    chosenVariant = variant;

                    break;
                }
            }

            return new ValueTask<FeatureVariant>(chosenVariant);
        }
    }
}
