using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using DemoERPApi.Tests.Fixtures;
using Microsoft.Extensions.Logging;
namespace DemoERPApi.Tests.Fixtures;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        /*
                builder.ConfigureAppConfiguration(
                    configuration =>
                    {
                        configuration.AddJsonFile(
                            "appsettings.Test.json",
                            optional: false,
                            reloadOnChange: false);
                    });
        */

        builder.ConfigureAppConfiguration(
      (context, config) =>
      {
          config.AddInMemoryCollection(
              new Dictionary<string, string>
              {
                    {
                    "ConnectionStrings:DemoERPConnection",
                    @"Server=localhost\SQLEXPRESS;Database=DemoERP;Trusted_Connection=True;TrustServerCertificate=True"
                    }
              });
      });

        builder.UseEnvironment("Test");

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddProvider(new TestLoggerProvider());
        });


    }

    public TestLoggerProvider LoggerProvider { get; } = new();

}