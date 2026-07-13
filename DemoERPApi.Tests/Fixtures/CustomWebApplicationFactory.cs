using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DemoERPApi.Tests.Fixtures;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {

        builder.ConfigureAppConfiguration(
            configuration =>
            {
                configuration.AddJsonFile(
                    "appsettings.Test.json",
                    optional: false,
                    reloadOnChange: false);
            });


        builder.UseEnvironment("Test");
    }
}