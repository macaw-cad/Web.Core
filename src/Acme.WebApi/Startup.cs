using Acme.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Web.Core.DependencyInjection;
using Web.Core.Infrastructure;
using Web.Core.WebApi.DependencyInjection;
using Web.Core.WebApi.Middleware;

namespace Acme.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiControllersWithJsonSerializerOptions(Configuration);
            services.AddTransient<ProblemDetailsFactory, ErrorDetailsProblemDetailsFactory>(); // must be called after `services.AddControllers();` as that is where the default factory is registered.            

            services.AddLogging();
            services.ConfigureApiVersioning();
            services.ConfigureSwaggerDocWithVersioning("Acme.WebApi - API Documentation", "Healthchecks on:<ul><li><a href='/hc'>Minimal info (/hc)</a></li><li><a href='/mon'>Detailed info (/mon)</a></li></ul>");

            services.AddHealthChecks()
                .ApplicationInfoHealthCheck("Acme.WebApi")
                ;
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<ErrorHandlingMiddleware>();

            if (!Environment.IsProduction())
            {
                app.UseSwaggerWithDocumentation(new []
                {
                    Assembly.GetEntryAssembly(),
                });
            }

            app.UseHealthCheckEndPoints();

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.AddPing();
            });
        }
    }
}
