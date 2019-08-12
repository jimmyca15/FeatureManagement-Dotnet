using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FeatureFlagDemo
{
    public class AssignUserMiddleware
    {

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
            ClaimsIdentity identity = httpContext.GetUser();

            if (identity == null)
            {
                char[] randomUserName = new char[6];

                for (int i = 0; i < randomUserName.Length; i++)
                {
                    randomUserName[i] = (char)('a' + (_random.Next() % 26));
                }

                _logger.LogInformation($"AssignUserMiddleware inward path.");

                string username = new string(randomUserName);

                identity = httpContext.SetUser(username);
            }

            httpContext.User = new ClaimsPrincipal(identity);

            //
            // Call the next middleware delegate in the pipeline 
            await _next.Invoke(httpContext);

            _logger.LogInformation($"AssignUserMiddleware outward path.");
        }
    }
}