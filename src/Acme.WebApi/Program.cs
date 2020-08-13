using Acme.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Acme.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AcmeProgram.CreateHostBuilder<Startup>(args).Build().Run();
        }
    }
}
