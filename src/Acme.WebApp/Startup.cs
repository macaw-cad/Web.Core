using Acme.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Web.Core.Infrastructure;
using Web.Core.WebApi.DependencyInjection;
using Web.Core.WebApi.Middleware;

namespace Acme.WebApp
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
            services.AddControllersWithViewsAndJsonSerializerOptions(Configuration);
            services.AddTransient<ProblemDetailsFactory, ErrorDetailsProblemDetailsFactory>(); // must be called after 'services.AddControllers();' as that is where the default factory is registered.            

            services.AddLogging();
            services.ConfigureSwaggerDocWithoutVersioning("Acme.WebApp - API Documentation");
        }

        public void Configure(IApplicationBuilder app)
        {       
            if (!Environment.IsProduction())
            {
                app.UseSwaggerWithDocumentation(new[]
                {
                    Assembly.GetAssembly(typeof(Acme.WebApiLibrary.Data.Controllers.DataController)),
                    Assembly.GetAssembly(typeof(Acme.WebApiLibrary.MoreData.Controllers.DataController)),
                });

                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseMiddleware<ErrorHandlingMiddleware>(); // ErrorHandlingMiddleware must be after UseExceptionHandler and/or UseDeveloperExceptionPage, otherwise no destiction can be between a json or html response

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
