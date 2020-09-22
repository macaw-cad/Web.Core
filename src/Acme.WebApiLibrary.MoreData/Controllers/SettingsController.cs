using Acme.Core.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using Web.Core.Configuration;
using Web.Core.Mvc;
using Web.Core.WebApi.Controllers;

namespace Acme.WebApiLibrary.MoreData.Controllers
{
    [ApiController]
    [Route("api/settings")]
    public class SettingsController : ApiControllerBase
    {
        private readonly ILogger<SettingsController> _logger;
        private readonly IConfigurationValidator _configValidator;
        private readonly AcmeSettings _acmeSettings;

        public SettingsController(ILogger<SettingsController> logger, IConfigurationValidator configValidator, IOptionsSnapshot<AcmeSettings> options)
        {
            _logger = logger;
            _configValidator = configValidator;
            _acmeSettings = options?.Value;
        }

        /// <summary>
        /// Displays all Amce settings and a summary of all validation errors (if any)
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>An array of groups with configuration information.</returns>
        [HttpGet("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SettingGroup[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ExceptionProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult Settings()
        {
            var groups = new List<SettingGroup>
            {
                CreateSettingGroup("Acme Settings", new List<string>
                {
                    $"BackgroundColor: {_acmeSettings.BackgroundColor}",
                    $"FontColor: {_acmeSettings.FontColor}",
                    $"FontSize: {_acmeSettings.FontSize}",
                    $"Message: {_acmeSettings.Message}",
                    $"SomethingImportant: {_acmeSettings.SomethingImportant}",
                })
            };

            var errors = _configValidator.Validate();
            if (errors.Any())
            {
                groups.Add(CreateSettingGroup("Validation Errors", errors));
            }

            groups.Add(CreateSettingGroup("All Settings", _configValidator.GetAllSettings()));

            return Ok(groups);
        }

        private SettingGroup CreateSettingGroup(string name, IEnumerable<string> items)
        {
            return new SettingGroup
            {
                GroupName = name,
                Items = items,
            };
        }
    }

    public class SettingGroup
    {
        public string GroupName { get; set; }
        public IEnumerable<string> Items { get; set; }
    }
}
