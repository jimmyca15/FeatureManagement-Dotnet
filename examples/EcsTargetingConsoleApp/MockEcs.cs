using System;
using System.Collections.Generic;

namespace Consoto.Banking.AccountService
{
    public class MockEcs
    {
        public const string UserIdKey = "UserId";

        private readonly Dictionary<string, string> BigSettings = new Dictionary<string, string>
        {
            { "Size", "400" },
            { "Color", "Green" }
        };

        private readonly Dictionary<string, string> SmallSettings = new Dictionary<string, string>
        {
            { "Size", "150" },
            { "Color", "Gray" }
        };

        public Dictionary<string, string> GetSettings(Dictionary<string, string> info)
        {
            if (info.TryGetValue(UserIdKey, out string userId))
            {
                switch (userId)
                {
                    case "Jeff":
                    case "Alicia":

                        return BigSettings;

                    case "Susan":
                    case "JohnDoe":

                        return SmallSettings;

                    default:

                        break;
                }
            }

            return new Dictionary<string, string>();
        }
    }
}
