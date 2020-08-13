using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Acme.Core
{
    public static class AcmeProgram
    {
        public static IHostBuilder CreateHostBuilder<T>(string[] args) where T : class =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<T>();
            });
    }
}
