using DemoERPApi.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoERPApi.Tests.Helpers
{
    internal class TestDatabase
    {
        public static void Reset(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context =
                scope.ServiceProvider.GetRequiredService<AppDbContext>();

            DbSeeder.Reset(context);
        }
    }
}