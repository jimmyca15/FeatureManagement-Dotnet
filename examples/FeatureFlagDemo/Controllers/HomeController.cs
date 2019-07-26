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
using System;

namespace FeatureFlagDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFeatureManager _featureManager;

        public HomeController(IFeatureManagerSnapshot featureSnapshot)
        {
            _featureManager = featureSnapshot;
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

        public IActionResult Privacy()
        {
            return View();
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
