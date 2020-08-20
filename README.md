## Introduction

In large projects we will encounter multiple ASP.NET Web applications. These Web application contain one or more of the following HTTP based applications:

- Web site
- Unversioned backend-for-frontend Web API service
- Versioned Web API service

To support common tasks for ASP.NET Web applications we created a project named `Web.Core` that contains functionality and a demo application for all of the above types of applications.

The solution contains two libraries `Web.Core` and `Web.Core.WebApi` with funtionality to be used with web applications and API's and a sample project called `Acme` to demonstrate all of the features of the libraries and a way how application logic can be shared between App and API.

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

The way error reporting is done is different between an Web application and a Web API. They both responds with a statuscode but the response body can differ. A Web application responds with an HTML page (Content-Type: text/html) and Web API reponds with JSON data (Content-Type: application/json).

A machine-readable format for specifying errors in HTTP API responses based on https://tools.ietf.org/html/rfc7807.

Unfortunately not all methods on the standard ControllerBase class respond in the same format and for that problem we created a custom `ErrorDetailsProblemDetailsFactory` to replace the standard 'ProblemDetailsFactory' and a middleware component `ErrorHandlingMiddleware` to handle unexpected exeptions. The middleware catches any unhandled exception and creates an response with code: 500 Internal Server Error. 

To make it easier in an API controller a custom base class `ApiControllerBase` is available that derives from the standard `ControllerBase`.

This base class implements the `ErrorDetailsProblemDetailsFactory` and overrides various methods *(like: BadRequest, NotFound, ect)* to use a object that implements `IErrorDetails` to reponse with a proper problem details output.

Any unhandled exception is translated into a `ExceptionProblemDetails` which contains detailed information about the exception. The detail level (Minimal, Moderate, Full) and inner exception depth can be controlled via configuration in the appsettings.

> *In a production enviroment is not a good practice to output the expection details, but for development and/or debugging is can be very usefull. So be carefull what settings to use!*


`public void ConfigureServices(IServiceCollection services)`
```cs
    services.AddTransient<ProblemDetailsFactory, ErrorDetailsProblemDetailsFactory>(); // must be called after 'services.AddControllers();' as that is where the default factory is registered.            
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
    app.UseMiddleware<ErrorHandlingMiddleware>(); // ErrorHandlingMiddleware must be after UseExceptionHandler and/or UseDeveloperExceptionPage, otherwise no destiction can be between a json or html response (only needed in a web-application with an inner api, in a webapi only application there is no need for the whole if block)
```

```appsettings.json```
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

More information about Problem Details Response: [REST API Error Handling - Problem Details Response](https://blog.restcase.com/rest-api-error-handling-problem-details-response/)



## Application Configuration / Validation

Read more about Configuration: [Configuration in ASP.NET Core](
https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1)

When the applications are run with the Kestrel web server, the appSettings.json files are read with `reloadOnChange` option set to: `true`.

We have a set of rules that we apply with respect to configuration files:

- We only use **Debug** and **Release** mode, no additional project configurations
- In **Debug** mode the value of the environment variable `ASPNETCORE_ENVIRONMENT` is `Development` and `IWebHostEnvironment.IsDevelopment()` returns `true`
- In **Release** mode the value of the environment variable `ASPNETCORE_ENVIRONMENT` is `Production` OR the the environment variable `ASPNETCORE_ENVIRONMENT`is undefined and `IWebHostEnvironment.IsProduction()` returns `true`
- There are only two configuration files in the folder of a project:
  - `appsettings.json` - the default application settings file that is always used
  - `appsettings.Development.json` - contains overrides for development only
- On the local development machine we always execute our code in Debug mode, which means that the `appsettings.Development.json` is always active
- The `appsettings.json` file specifies:
  - Settings that are used equally in all environments (local development - in Debug mode, deployment environments - in Release mode)
  - Settings that must be replaced:
    - In local development (Debug mode) by using `appsettings.Development.json` 
    - In deployed environments (Release mode) by replacing the values in the `appsettings.json` OR by overriding the values using environment variables (necessary when running in a Docker container)
- On deployment all `appsettings.*.json` files are deleted

All application settings to be replaced are marked with `{{ ... }}` in the `appsettings.json file, e.g.:

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

These `{{...}}` replacement values are handled in the deployment pipeline (by replacement in the `appsettings.json` file, or by override through environment variables).

Note that a value like `BlobStorage.ConnectionString` can be replaced by a connection string that contains references to a secret from the Keyfault. In that case these secrets are identified by a value `$(keyfault_key)` and will also be replaced in the deployment pipeline.

The value of `VersionInfo:BuildVersion` has a special notation `#{BUILDVERSION_TOKEN}#`. This value is not replaced for local development (and gives no validation errors), but is replaced in the `appsettings.json` file in the build pipeline.

To ensure all `{{...}}` replacement values are replaced we have an `ConfigurationValidator` class that can validate all settings.

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

Read more on ASP.NET API Versioning on github:
https://github.com/microsoft/aspnet-api-versioning


## API Documentation

For API documentation whe choose a NSWag: The Swagger/OpenAPI toolchain for .NET, ASP.NET Core and TypeScript.

To have detailed information of your controllers and actions, turn on the XML documentation file via the project properties (tab: Build) of the project containing the controllers and specified the assembly in UseSwaggerWithDocumentation extension.

* Use the appropriated extension when you do or don't use API version!

`public void ConfigureServices(IServiceCollection services)`
```cs
    services.ConfigureSwaggerDocWithVersioning("My Title", "My description about this API");
```
or
```cs
    services.ConfigureSwaggerDocWithoutVersioning("My Title", "My description about this API");

```

`public void Configure(IApplicationBuilder app)`
```cs
    app.UseSwaggerWithDocumentation(new []
    {
        Assembly.GetEntryAssembly(),
    });
```

Read more on NSWag on github:
https://github.com/RicoSuter/NSwag


## Health monitoring

- ApplicationInfoHealthCheck
  - AddApplicationInfoHealthCheck extension
- ApplicationEndpointHealthCheck
  - AddApplicationEndpointsHealthCheck extension
- UseHealthCheckEndPoints extension
  
`ApplicationInfoHealthCheck` is a healthcheck that always returns `Healthy` and can be used as an endpoint to see if the application is up-and-running and contains some information about it.

`ApplicationEndpointHealthCheck` is a healthcheck that does a HEAD request to a specified URI and returns Healty when the request has a Ok (200) response.

`AddApplicationEndpointsHealthCheck` is a extension that uses appsettings to configure which endpoints should be requested. 


`public void ConfigureServices(IServiceCollection services)`
```cs
    services.AddHealthChecks()
        .ApplicationInfoHealthCheck("My application name")
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

`UseHealthCheckEndPoints` is a extension that creates two endpoints health monitoring.
- `/hc` - from minimal information and only returns `Healthy` or `UnHealthy`.
- `/mon` - from detailed information on all health check that are added to monitor.

`public void Configure(IApplicationBuilder app)`
```cs
    app.UseHealthCheckEndPoints();
```

Read more about Configuration: [Health monitoring](
https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/monitor-app-health)


## Ping

To see if application is up-and-running add the Ping lightweight endpoint. This will create a route: `/ping` which should be called with a `HEAD request`.

`public void Configure(IApplicationBuilder app)`
```cs
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.AddPing();
    });
```


## Default setup of all ASP.NET Web applications

By default all ASP.NET Web and API applications contain two main entry point files:

- Program.cs
- Startup.cs

See the `Amce.WebApp` and `Acme.WebApi` for an example!
