using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;
using System;
using System.Collections.Generic;

namespace FeatureFlagDemo
{
    public class HttpUserAccessor : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpUserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string UserId => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        public ICollection<string> Audiences { get; set; } = new List<string>();
    }
}
