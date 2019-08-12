using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FeatureFlagDemo
{
    public static class HttpExtensions
    {
        private const string SessionUsernameKey = "username";

        public static ClaimsIdentity SetUser(this HttpContext context, string username)
        {
            context.Session.SetString(SessionUsernameKey, username);

            return new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, username)
            });
        }

        public static ClaimsIdentity GetUser(this HttpContext context)
        {
            string username = context.Session.GetString(SessionUsernameKey);

            ClaimsIdentity identity = null;

            if (!string.IsNullOrEmpty(username))
            {
                identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, username)
                });
            }

            return identity;
        }
    }
}
