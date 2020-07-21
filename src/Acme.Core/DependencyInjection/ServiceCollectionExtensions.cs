using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using Web.Core.Mvc;

namespace Acme.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private const bool respectBrowserAcceptHeader = true;

        private static JsonSerializerOptions CreateJsonSerializerOptions(IConfiguration configuration)
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
            };

            jsonSerializerOptions.Converters.Add(new ErrorProblemDetailsJsonConverterFactory());
            jsonSerializerOptions.Converters.Add(
                new ExceptionProblemDetailsJsonConverter(
                    configuration.GetSection(ExceptionProblemDetailsOptions.ExceptionProblemDetails).Get<ExceptionProblemDetailsOptions>()
                ));

            return jsonSerializerOptions;
        }

        public static IServiceCollection AddApiControllersWithJsonSerializerOptions(this IServiceCollection services, IConfiguration configuration)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var jsonSerializerOptions = CreateJsonSerializerOptions(configuration);

            // Add JsonSerializerOptions to the controllers and to the DI-container, otherwise the same settings won't available in (middleware) services
            services.AddTransient(_ => Options.Create(jsonSerializerOptions));
            services.AddControllers(mvcOptions => mvcOptions.RespectBrowserAcceptHeader = respectBrowserAcceptHeader)
                .AddJsonOptions(jsonOptions => CopySerializerOptions(jsonOptions, jsonSerializerOptions)
                );

            return services;
        }

        public static IServiceCollection AddControllersWithViewsAndJsonSerializerOptions(this IServiceCollection services, IConfiguration configuration)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var jsonSerializerOptions = CreateJsonSerializerOptions(configuration);

            // Add JsonSerializerOptions to the controllers and to the DI-container, otherwise the same settings won't available in (middleware) services
            services.AddTransient(_ => Options.Create(jsonSerializerOptions));
            services.AddControllersWithViews(mvcOptions => mvcOptions.RespectBrowserAcceptHeader = respectBrowserAcceptHeader)
                .AddJsonOptions(jsonOptions => CopySerializerOptions(jsonOptions, jsonSerializerOptions)
                );

            return services;
        }

        private static void CopySerializerOptions(JsonOptions jsonOptions, JsonSerializerOptions jsonSerializerOptions)
        {
            jsonOptions.JsonSerializerOptions.IgnoreNullValues = jsonSerializerOptions.IgnoreNullValues;
            jsonOptions.JsonSerializerOptions.WriteIndented = jsonSerializerOptions.WriteIndented;
            jsonOptions.JsonSerializerOptions.PropertyNameCaseInsensitive = jsonSerializerOptions.PropertyNameCaseInsensitive;
            foreach (var converter in jsonSerializerOptions.Converters)
            {
                jsonOptions.JsonSerializerOptions.Converters.Add(converter);
            }
        }
    }
}
