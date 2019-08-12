using System.Collections.Generic;

namespace Microsoft.FeatureManagement
{
    public interface IUserContext
    {
        string UserId { get; }

        ICollection<string> Audiences { get; }
    }
}
