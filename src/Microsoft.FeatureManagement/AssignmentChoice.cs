using Microsoft.Extensions.Configuration;

namespace Microsoft.FeatureManagement
{
    public class AssignmentChoice
    {
        public string Name { get; set; }

        public IConfiguration Parameters { get; set; }
    }
}
