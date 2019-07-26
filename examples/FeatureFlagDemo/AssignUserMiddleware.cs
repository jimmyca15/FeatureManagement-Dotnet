using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FeatureFlagDemo
{
    public class AssignUserMiddleware
    {
        private const string SessionUsernameKey = "username";
        private const string SessionUserIdKey = "userId";

        //
        // The middleware delegate to call after this one finishes processing
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private Random _random = new Random();

        public AssignUserMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<AssignUserMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            string username = httpContext.Session.GetString(SessionUsernameKey);
            string userId = httpContext.Session.GetString(SessionUserIdKey);

            if (string.IsNullOrEmpty(username))
            {
                char[] randomUserName = new char[6];

                for (int i = 0; i < randomUserName.Length; i++)
                {
                    randomUserName[i] = (char)('a' + (_random.Next() % 26));
                }

                _logger.LogInformation($"AssignUserMiddleware inward path.");

                username = new string(randomUserName);
                userId = Guid.NewGuid().ToString();

                httpContext.Session.SetString(SessionUsernameKey, username);
                httpContext.Session.SetString(SessionUserIdKey, userId);
            }

            var claims = new[] {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.NameIdentifier, userId)
                };

            ClaimsIdentity identity = new ClaimsIdentity(claims);

            httpContext.User = new ClaimsPrincipal(identity);

            //
            // Call the next middleware delegate in the pipeline 
            await _next.Invoke(httpContext);

            _logger.LogInformation($"AssignUserMiddleware outward path.");
        }
    }
}