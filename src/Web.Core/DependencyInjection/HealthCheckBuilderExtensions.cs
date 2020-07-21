using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Web.Core.HealthChecks;

namespace Web.Core.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder ApplicationInfoHealthCheck(this IHealthChecksBuilder builder, string name, IEnumerable<string> tags = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Argument can not be null or empty string.", nameof(name));
            }

            builder.AddCheck<ApplicationInfoHealthCheck>(name, tags: tags);

            return builder;
        }
    }
}
