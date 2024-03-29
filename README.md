## Introduction

In large projects we will encounter multiple ASP.NET Core 6.0 Web applications. Those Web application contain one or more of the following HTTP based applications:

- Web site
- Unversioned backend-for-frontend Web API service
- Versioned Web API service

To support common tasks for ASP.NET Web applications we created a project named `Web.Core` that contains functionality and a demo application for all of the above types of applications.

The solution contains two libraries `Web.Core` and `Web.Core.WebApi` with functionality to be used with web applications and APIs and a sample project called `Acme` to demonstrate all of the features of the libraries and a way how application logic can be shared between App and API.

The features we now support are:

- Error handling / reporting
  - Base class for API controllers
  - ErrorDetailsProblemDetailsFactory
  - ErrorHandlingMiddleware
  - ApiControllerBase
  - IErrorDetails
  - ExceptionProblemDetails
- Application configuration / validation
- API documentation
- API versioning
- Health monitoring
- Ping

## Exception handling / reporting

The way error reporting is done is different between a Web application and a Web API. They both responds with a HTTP statuscode (e.g. 200, 401, 500) but the response body can differ. A Web application responds with an HTML page (`Content-Type: text/html`) and a Web API responds with JSON data (`Content-Type: application/json`), a machine-readable format for specifying errors in HTTP API responses based on https://tools.ietf.org/html/rfc7807.

Unfortunately not all methods on the standard `ControllerBase` class respond in the same format and for that problem we created a custom `ErrorDetailsProblemDetailsFactory` to replace the standard `ProblemDetailsFactory` and a middleware component `ErrorHandlingMiddleware` to handle unexpected exceptions. The middleware catches any unhandled exception and creates a response with statuscode: `500 Internal Server Error`.

To make it easier to develop an API controller, a custom base class `ApiControllerBase` is available that derives from the standard `ControllerBase`.

This base class implements the `ErrorDetailsProblemDetailsFactory` and overrides various methods like: `BadRequest`, `NotFound`, etc to use an object that implements `IErrorDetails` to respond with a proper problem details output.

Any unhandled exception is translated into an `ExceptionProblemDetails` response which contains detailed information about the exception. The detail level (`Minimal`, `Moderate`, `Full`) and inner exception depth can be controlled via configuration in the application settings.

> _In a production environment it is not a good practice to output the exception details, but for development and/or debugging is can be very useful. So be careful what settings to use!_

`public void ConfigureServices(IServiceCollection services)`

```cs
    services.AddTransient<ProblemDetailsFactory, ErrorDetailsProblemDetailsFactory>(); // must be called after 'services.AddControllers();' where the default factory is registered.
```

`public void Configure(IApplicationBuilder app)`

```cs
    if (!Environment.IsProduction())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
    }
    app.UseMiddleware<ErrorHandlingMiddleware>(); // ErrorHandlingMiddleware must be called after UseExceptionHandler and/or UseDeveloperExceptionPage, otherwise no destinction can be made between a json or html response (only needed in a web-application also containing an API, in a WebApi only application there is no need for the above if-block
```

`appsettings.json`

```json
  "ExceptionProblemDetails": {
    "Details": "Minimal",
    "Depth": 1
  },
```

`example controller`

```cs
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/data")]
    public class DataController : ApiControllerBase
    {
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorProblemDetails<AcmeDataErrorDetails>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ExceptionProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult Get(int value = 0)
        {
            if (value == 0)
            {
                return BadRequest(new AcmeDataErrorDetails
                {
                    BooleanValue = true,
                    DateValue = DateTime.Now,
                    DecimalValue = 26.3M,
                    IntValue = 42,
                    StringValue = "Lorem Ipsum Honda Magna",
                });
            }
            if (value == 1)
            {
                throw new Exception("Something went very wrong!");
            }

            return Ok();
        }
    }
```

```cs
    public class AcmeDataErrorDetails : IErrorDetails
    {
        public int IntValue { get; set; }
        public decimal DecimalValue { get; set; }
        public string StringValue { get; set; }
        public DateTime DateValue { get; set; }
        public bool BooleanValue { get; set; }
    }
```

More information about Problem Details Response see: [REST API Error Handling - Problem Details Response](https://blog.restcase.com/rest-api-error-handling-problem-details-response/)

## Application Configuration / Validation

To read more about ASP.NET Core Configuration see: [Configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)

When the applications are run with the Kestrel web server, the `appsettings.json` files are read with the `reloadOnChange` option set to: `true`.

We have a set of rules that we apply with respect to configuration files:

- We only use **Debug** and **Release** mode, no additional project configurations
- In **Debug** mode the value of the environment variable `ASPNETCORE_ENVIRONMENT` is `Development` and `IWebHostEnvironment.IsDevelopment()` returns `true`
- In **Release** mode the value of the environment variable `ASPNETCORE_ENVIRONMENT` is `Production` OR the the environment variable `ASPNETCORE_ENVIRONMENT`is undefined and `IWebHostEnvironment.IsProduction()` returns `true`
- There are only two configuration files in the folder of a project:
  - `appsettings.json` - the default application settings file that is always used
  - `appsettings.Development.json` - contains overrides for development only
- On the local development machine we always execute our code in Debug mode, which means that the `appsettings.Development.json` is always active on top of `appsettings.json`
- The `appsettings.json` file specifies:
  - Settings that are used equally in all environments (local development - in Debug mode, deployment environments - in Release mode)
  - Settings that must be replaced:
    - In local development (Debug mode) by using `appsettings.Development.json`
    - In deployed environments (Release mode) by replacing the values in the `appsettings.json` OR by overriding the values using environment variables (necessary when running in a Docker container)
- On deployment all `appsettings.*.json` files should be deleted, so the `apsettings.Development.json` configuration settings don't kick in when setting `ASPNETCORE_ENVIRONMENT` to `Development` on a deployed environment (we have been there:-))

All application settings to be replaced are marked with `{{ ... }}` in the `appsettings.json` file, e.g.:

```json
{
  "AzureAd": {
    "Instance": "{{Replace with Azure AD instance, like https://login.microsoftonline.com/}}",
    "Domain": "{{Replace with Azure AD domain, like mydomain.onmicrosoft.com}}",
    "TenantId": "{{Replace with tentant id, this is a GUID}}",
    "ClientId": "{{Replace with client id, this is a GUID}}"
  },
  "ApplicationInsights": {
    "InstrumentationKey": "{{Replace with instrumentation key, this is a GUID}}"
  },
  "BlobStorage": {
    "ConnectionString": "{{Replace with blob storage connection string}}",
    "Containers": {
      "Content": "content"
    }
  },
  "VersionInfo": {
    "BuildVersion": "#{BUILDVERSION_TOKEN}#"
  }
}
```

These `{{...}}` replacement values should be handled in the deployment pipeline by replacement in the `appsettings.json` file, or by override through environment variables.

Note that a value like `BlobStorage.ConnectionString` can be replaced by a connection string that contains references to a secret from the Keyfault. In that case these secrets are identified by a value `$(keyfault_key)` and will also be replaced in the deployment pipeline.

The value of `VersionInfo:BuildVersion` has a special notation `#{BUILDVERSION_TOKEN}#`. This value is not replaced for local development (and gives no validation errors), but is replaced in the `appsettings.json` file in the build pipeline.

To ensure all `{{...}}` replacement values are replaced or overridden, we have a `ConfigurationValidator` class that can validate all settings at runtime.

`IConfigurationValidator.Validate()` returns a string array containing any errors.

Add this validator to the DI container via:

`public void ConfigureServices(IServiceCollection services)`

```cs
    services.AddTransient<IConfigurationValidator, ConfigurationValidator>();
```

## API Versioning

Turn on API Versioning via:

`public void ConfigureServices(IServiceCollection services)`

```cs
    services.ConfigureApiVersioning();
```

Decorate the API controllers with the `ApiVersion` and `Route` attributes

eg:

```cs
    [ApiController]
    [ApiVersion("1", Deprecated = false)]
    [Route("api/v{version:apiVersion}/data")]
    public class DataController : ApiControllerBase
    {
        ...
    }
```

To read more on ASP.NET API Versioning see: https://github.com/microsoft/aspnet-api-versioning

## API security
To see if a user is authorized to access a api a ApiAuthorize attribute is added. This is needed to produce a 401 statuscode, instead of redirecting to a login page. *Which is expected for a web page, but not for a api-call*. The attribute can be used on action or controller level.

```cs
    [ApiAuthorize]
    [HttpGet("secure")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ExceptionProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult Secure()
    {
        ...
    }   
```
 or

```cs
    [ApiController]
    [ApiAuthorize]    
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/data")]
    public class DataController : ApiControllerBase
    {
        ...
    }    
```

## API Documentation

For API documentation we chose for NSWag: The Swagger/OpenAPI toolchain for .NET, ASP.NET Core and TypeScript. An important reason for this selection is that NSwag provides the capabilities to generate clients in TypeScript or C# for the consumption of the API.

To have detailed information of your controllers and actions, turn on the XML documentation file via the project properties (tab: Build) of the project containing the controllers, and specified the assembly in the UseSwaggerWithDocumentation extension.

`public void ConfigureServices(IServiceCollection services)`

```cs
    services.ConfigureSwaggerDoc("My Title", "My description about this API");
```

`public void Configure(IApplicationBuilder app)`

```cs
    app.UseSwaggerWithDocumentation(new []
    {
        Assembly.GetEntryAssembly(),
    });
```

For more information on NSWag see: https://github.com/RicoSuter/NSwag

## Health monitoring

- ApplicationInfoHealthCheck
  - AddApplicationInfoHealthCheck extension
- ConfigurationValidationHealthCheck
  - AddConfigurationValidationHealthCheck extension
- ApplicationEndpointHealthCheck
  - AddApplicationEndpointsHealthCheck extension
- UseHealthCheckEndPoints extension

`ApplicationInfoHealthCheck` is a healthcheck that always returns `Healthy` and can be used as an endpoint to see if the application is up-and-running and contains some information about it.

`ConfigurationValidationHealthCheck` is a healthcheck that uses the ConfigurationValidator to validate if all the application configuration replacements are done.

`ApplicationEndpointHealthCheck` is a healthcheck that does a HEAD request to a specified URI and returns `Healthy` when the request has an OK (200) response.

`AddApplicationEndpointsHealthCheck` is an extension that uses `appsettings.json` to configure which endpoints should be requested.

`public void ConfigureServices(IServiceCollection services)`

```cs
    services.AddHealthChecks()
        .ApplicationInfoHealthCheck("My application name")
        .AddConfigurationValidationHealthCheck("configuration")
        .AddApplicationEndpointsHealthCheck("ping", "HealthChecks").Get<HealthCheckOptions>());
```

`appsettings.json`

```json
  "HealthChecks": {
    "ApplicationEndpoints": [
      {
        "Name": "Acme.WebApi",
        "Url": "{{replace-with-application-ping-url}}",
        "Timeout": 5000
      }
    ]
  }
```

`UseHealthCheckEndPoints` is an extension that creates two endpoints for health monitoring:

- `/hc` - from minimal information and only returns `Healthy` or `UnHealthy`.
- `/mon` - from detailed information on all health checks that are added to monitor.

`public void Configure(IApplicationBuilder app)`

```cs
    app.UseHealthCheckEndPoints();
```

For more information on configuration see: [Health monitoring](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/monitor-app-health)

## Ping

To see if an application is up-and-running add the Ping lightweight endpoint. This will create a route: `/ping` which should be called with a `HEAD request`.

`public void Configure(IApplicationBuilder app)`

```cs
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.AddPing();
    });
```

## Default setup of all ASP.NET Core Web applications

By default all ASP.NET Core Web and API applications contain two main entry point files:

- Program.cs
- Startup.cs

See the `Amce.WebApp`, `Acme.WebSpa` and `Acme.WebApi` applications for an example!
