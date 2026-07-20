using DemoERPApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DemoERPApi.Tests.Integration;


/// Custom WebApplicationFactory for integration testing.
/// Configures the application to use an in-memory database instead of
/// the real database, ensuring tests are isolated and repeatable.

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    
    /// Configures the web host for testing.
    /// Overrides the default database configuration to use an in-memory database.
    
    /// <param name="builder">Web host builder for configuration</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set the environment to "Test" to use test-specific configurations
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Remove all existing AppDbContext registrations to avoid conflicts
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));

            // Register an in-memory database for testing
            // Using a consistent database name ensures all tests use the same instance
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("DemoERPConnection");
            });

            // Build the service provider and seed the database
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Reset the database to a clean state before each test run
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // Seed the database with test data
            DbSeeder.Seed(db);
        });
    }
}