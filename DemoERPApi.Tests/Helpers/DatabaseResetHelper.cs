using DemoERPApi.Data;
using Microsoft.Extensions.DependencyInjection;

namespace DemoERPApi.Tests.Helpers;

public static class DatabaseResetHelper
{
    public static void Reset(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context =
            scope.ServiceProvider
                 .GetRequiredService<AppDbContext>();

        DbSeeder.Reset(context);
    }
}