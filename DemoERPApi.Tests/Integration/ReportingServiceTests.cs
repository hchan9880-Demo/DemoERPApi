using DemoERPApi.Data;
using DemoERPApi.Services;
using DemoERPApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;

namespace DemoERPApi.Tests.Services;


/// Unit tests for the ReportingService.
/// Validates that reporting calculations and summaries are accurate
/// using an in-memory database for isolated testing.

public class ReportingServiceTests
{
    private readonly AppDbContext _context;

    // Test data constants
    private const string ActiveCustomerId = "CRM100";
    private const string ActiveFirstName = "John";
    private const string ActiveLastName = "Smith";
    private const string ActiveEmail = "john@test.com";
    private const string ActivePhone = "1234567890";

    private const string DeletedCustomerId = "CRM101";
    private const string DeletedFirstName = "Mary";
    private const string DeletedLastName = "Lee";
    private const string DeletedEmail = "mary@test.com";
    private const string DeletedPhone = "5555555555";

    
    /// Initializes a new instance of the <see cref="ReportingServiceTests"/> class.
    /// Creates a fresh in-memory database and seeds it with test data.
    
    public ReportingServiceTests()
    {
        // Create a new in-memory database with a unique name for test isolation
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        SeedData();
    }

    #region Test Data Helpers

    
    /// Seeds the in-memory database with test customer data.
    /// Creates one active customer and one soft-deleted customer.
    
    private void SeedData()
    {
        _context.Customers.AddRange(
            // Active customer (IsDeleted = false)
            new Customers
            {
                CRMCustomerID = ActiveCustomerId,
                FirstName = ActiveFirstName,
                LastName = ActiveLastName,
                Email = ActiveEmail,
                Phone = ActivePhone,
                IsDeleted = false
            },
            // Soft-deleted customer (IsDeleted = true)
            new Customers
            {
                CRMCustomerID = DeletedCustomerId,
                FirstName = DeletedFirstName,
                LastName = DeletedLastName,
                Email = DeletedEmail,
                Phone = DeletedPhone,
                IsDeleted = true
            }
        );

        _context.SaveChanges();
    }

    
    /// Creates an instance of the ReportingService for testing.
    
    /// <returns>ReportingService instance with the in-memory context</returns>
    private ReportingService CreateReportingService()
    {
        return new ReportingService(_context);
    }

    #endregion

    #region Reporting Tests

    
    /// Tests that GetCustomerSummaryAsync returns accurate customer counts.
    /// 
    /// Workflow:
    /// 1. Seed database with test data (1 active, 1 deleted customer)
    /// 2. Create ReportingService instance
    /// 3. Call GetCustomerSummaryAsync
    /// 4. Verify the summary counts match expected values
    /// 
    /// This test ensures:
    /// - Total customer count includes all customers (active + deleted)
    /// - Active customer count only includes non-deleted customers
    /// - Deleted customer count only includes soft-deleted customers
    /// - The service correctly applies the IsDeleted filter
    
    [Fact]
    public async Task REP006_GetCustomerSummary_ShouldReturnCorrectCounts()
    {
        // Arrange: Create the service with seeded data
        var service = CreateReportingService();

        // Act: Get the customer summary
        var result = await service.GetCustomerSummaryAsync();

        // Assert: Verify the summary contains expected counts
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCustomers);      // Total: 2 customers (1 active + 1 deleted)
        Assert.Equal(1, result.ActiveCustomers);     // Active: Only the non-deleted customer
        Assert.Equal(1, result.DeletedCustomers);    // Deleted: Only the soft-deleted customer
    }

    #endregion
}