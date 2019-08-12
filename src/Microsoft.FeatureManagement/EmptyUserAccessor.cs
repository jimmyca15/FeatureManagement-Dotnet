using System.Collections.Generic;

namespace Microsoft.FeatureManagement
{
    class EmptyUserAccessor : IUserContext
    {
        public string UserId { get; set; } = null;

        public ICollection<string> Audiences { get; set; } = new List<string>();
    }
}
