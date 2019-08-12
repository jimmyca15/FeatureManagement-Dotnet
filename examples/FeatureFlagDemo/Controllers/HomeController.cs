// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FeatureFlagDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Security.Claims;
using System;

namespace FeatureFlagDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFeatureManager _featureManager;
        private readonly IUserContext _userAccessor;

        public HomeController(IFeatureManager featureSnapshot, IUserContext userAccessor)
        {
            _featureManager = featureSnapshot;
            _userAccessor = userAccessor;
        }

        [FeatureGate(MyFeatures.Home)]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            if (_featureManager.IsEnabled(nameof(MyFeatures.CustomViewData)))
            {
                ViewData["Message"] = $"This is FANCY CONTENT you can see only if '{nameof(MyFeatures.CustomViewData)}' is enabled.";
            };

            return View(_featureManager.GetConfiguration(nameof(MyFeatures.AboutBox)).Get<AboutBoxSettings>());
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [FeatureGate(MyFeatures.Beta)]
        public IActionResult Beta()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult MultiUser()
        {
            List<MultiUserSettings> settings = new List<MultiUserSettings>();

            var user = HttpContext.GetUser();

            Random random = new Random();

            for (int i = 0; i < 100; i++)
            {
                char[] randomUserName = new char[6];

                for (int j = 0; j < randomUserName.Length; j++)
                {
                    randomUserName[j] = (char)('a' + (random.Next() % 26));
                }

                string username = new string (randomUserName);
                string ring = "Ring0";

                if (i == 0)
                {
                    username = "Drago";
                }

                if (i == 10)
                {
                    username = "Zhenlan";
                }

                if (i == 40)
                {
                    username = "Jimmy";
                }

                if (i >= 10)
                {
                    ring = "Ring1";
                }

                if (i >= 40)
                {
                    ring = "Ring2";
                }

                HttpContext.User = new ClaimsPrincipal(HttpContext.SetUser(username));

                _userAccessor.Audiences.Clear();

                _userAccessor.Audiences.Add(ring);

                settings.Add(new MultiUserSettings
                {
                    UserName = username,
                    Audience = ring,
                    AboutBoxSettings = _featureManager.GetConfiguration(nameof(MyFeatures.AboutBox)).Get<AboutBoxSettings>()
                });
            }

            HttpContext.SetUser(user.Name);

            return View(settings);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Login()
        {
            HttpContext.Session.Clear();

            HttpContext.SetUser(HttpContext.Request.Form["username"]);

            return Redirect("~/home");
        }

        public IActionResult ClearSession()
        {
            HttpContext.Session.Clear();

            return Redirect("~/home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
