using PCPalService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.Hosting.WindowsServices;


namespace PCPalService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args)
              .UseWindowsService()
              .ConfigureServices((hostContext, services) =>
              {
                  services.AddHostedService<Worker>();
              });
    }
}
