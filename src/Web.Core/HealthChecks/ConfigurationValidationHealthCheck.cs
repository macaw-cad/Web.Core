using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Web.Core.Configuration;

namespace Web.Core.HealthChecks
{
    public class ConfigurationValidationHealthCheck : IHealthCheck
    {
        private readonly IConfigurationValidator _configurationValidator;

        public ConfigurationValidationHealthCheck(IConfigurationValidator configurationValidator)
        {
            _configurationValidator = configurationValidator ?? throw new ArgumentNullException(nameof(configurationValidator));
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Validate());
        }

        private HealthCheckResult Validate()
        {
            var validationErrors = _configurationValidator.Validate();
            if (validationErrors.Any())
            {
                var data = new Dictionary<string, object>();
                foreach (var error in validationErrors)
                {
                    data.Add($"error-{data.Count + 1}", error);
                }

                return HealthCheckResult.Unhealthy("Application configuration is invalid!", data: data);
            }

            return HealthCheckResult.Healthy("Application configuration is valid!");
        }
    }
}
