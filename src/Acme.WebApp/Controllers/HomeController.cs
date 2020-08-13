using Acme.Core.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using Web.Core.Configuration;

namespace Acme.WebApp.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfigurationValidator _configValidator;
        private readonly AcmeSettings _acmeSettings;

        public HomeController(ILogger<HomeController> logger, IConfigurationValidator configValidator, IOptionsSnapshot<AcmeSettings> options)
        {
            _logger = logger;
            _configValidator = configValidator;
            _acmeSettings = options?.Value;
        }

        public IActionResult Index()
        {
            ViewData["settings"] = _acmeSettings;
            ViewData["configErrors"] = _configValidator.Validate();
            ViewData["configLines"] = _configValidator.GetAllSettings();

            return View();
        }

        public IActionResult Privacy(int t = 0)
        {
            if (t != 0)
            {
                throw new Exception("some stupid programmer exception!");
            }

            return View();
        }
    }
}
