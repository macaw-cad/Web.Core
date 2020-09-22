using Acme.Core;
using Acme.Core.DependencyInjection;
using Acme.Core.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Web.Core.Configuration;
using Web.Core.DependencyInjection;
using Web.Core.Infrastructure;
using Web.Core.WebApi.DependencyInjection;
using Web.Core.WebApi.Middleware;

namespace Amce.WebSpa
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AcmeSettings>(Configuration.GetSection(AcmeConstants.Configuration.Sections.AcmeSettings));

            services.AddControllersWithViewsAndJsonSerializerOptions(Configuration);
            services.AddTransient<ProblemDetailsFactory, ErrorDetailsProblemDetailsFactory>(); // must be called after 'services.AddControllers();' as that is where the default factory is registered.            

            services.AddLogging();
            services.ConfigureSwaggerDoc(
                Configuration.GetValue<string>(AcmeConstants.Configuration.Swagger.Title),
                Configuration.GetValue<string>(AcmeConstants.Configuration.Swagger.Description)
                );

            services.AddHealthChecks()
                .AddApplicationInfoHealthCheck("Acme.WebSpa")
                ;

            services.AddTransient<IConfigurationValidator, ConfigurationValidator>();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (!Environment.IsProduction())
            {
                app.UseSwaggerWithDocumentation(new[]
                {
                    Assembly.GetAssembly(typeof(Acme.WebApiLibrary.MoreData.Controllers.SettingsController)),
                });

                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseMiddleware<ErrorHandlingMiddleware>(); // ErrorHandlingMiddleware must be after UseExceptionHandler and/or UseDeveloperExceptionPage, otherwise no destiction can be between a json or html response

            app.UseHealthCheckEndPoints();

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.AddPing();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (Environment.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
