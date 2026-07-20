using DemoERPApi.Data;
using DemoERPApi.Models;
using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Xunit;
using static DemoERPApi.Tests.Helpers.TestData;

namespace DemoERPApi.Tests.Integration.Logging;


/// Comprehensive integration tests for audit logging functionality.
/// Validates that all CRUD operations generate proper audit records
/// with correct metadata including timestamps, user info, and request tracking.
/// 
/// Test Coverage:
/// AUDIT001    POST Customer creates audit record with Action = CREATE
/// AUDIT002    PUT Customer stores OldValues and NewValues
/// AUDIT003    DELETE Customer creates audit record with Action = DELETE
/// AUDIT004    Unauthorized user cannot access audit endpoint
/// AUDIT005    Audit timestamp exists and is valid
/// AUDIT006    ChangedBy is populated with correct username
/// AUDIT007    RequestId is stored for request tracking
/// AUDIT008    Admin can view audit logs

public class AuditTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    // Test data constants
    private const string TestPhone = "6041234567";
    private const string TestFirstName = "Audit";
    private const string TestLastName = "Customer";

    
    /// Initializes a new instance of the <see cref="AuditTests"/> class.
    /// Sets up the test client with admin authentication for audit operations.
    
    /// <param name="factory">Custom web application factory for testing</param>
    public AuditTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        TestAuthHelper.SetAdminToken(_client);
    }

    #region Helper Methods

    
    /// Creates a customer for testing purposes.
    
    /// <param name="customerId">Customer CRM ID</param>
    /// <param name="firstName">First name</param>
    /// <param name="lastName">Last name</param>
    /// <param name="email">Email address</param>
    /// <param name="phone">Phone number</param>
    /// <returns>HTTP response from the sync endpoint</returns>
    private async Task<HttpResponseMessage> CreateCustomerAsync(
        string customerId,
        string firstName = TestFirstName,
        string lastName = TestLastName,
        string email = "audit@test.com",
        string phone = TestPhone)
    {
        var request = new CustomersDto
        {
            CRMCustomerID = customerId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone
        };

        return await _client.PostAsJsonAsync("/api/Customer/sync", request);
    }

    
    /// Updates a customer for testing purposes.
    
    /// <param name="customerId">Customer CRM ID</param>
    /// <param name="updatedCustomer">Updated customer data</param>
    /// <returns>HTTP response from the update endpoint</returns>
    private async Task<HttpResponseMessage> UpdateCustomerAsync(
        string customerId,
        CustomersDto updatedCustomer)
    {
        return await _client.PutAsJsonAsync($"/api/Customer/{customerId}", updatedCustomer);
    }

    
    /// Deletes a customer for testing purposes.
    
    /// <param name="customerId">Customer CRM ID to delete</param>
    /// <returns>HTTP response from the delete endpoint</returns>
    private async Task<HttpResponseMessage> DeleteCustomerAsync(string customerId)
    {
        return await _client.DeleteAsync($"/api/Customer/{customerId}");
    }

    
    /// Retrieves the latest audit record for a specific entity.
    
    /// <param name="entityId">Entity ID to search for</param>
    /// <param name="action">Optional action filter (CREATE, UPDATE, DELETE)</param>
    /// <returns>Audit log record or null if not found</returns>
    private async Task<AuditLogs?> GetLatestAuditRecordAsync(
        string entityId,
        string? action = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var query = db.AuditLogs.Where(x => x.EntityId == entityId);

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(x => x.Action == action);
        }

        return await query
            .OrderByDescending(x => x.ChangedDate)
            .FirstOrDefaultAsync();
    }

    
    /// Gets the database context for direct queries.
    
    /// <returns>AppDbContext instance</returns>
    private async Task<AppDbContext> GetDbContextAsync()
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    #endregion

    #region Audit CRUD Tests


    /// AUDIT001: Verifies that creating a customer generates a CREATE audit record.
    /// 
    /// Workflow:
    /// 1. Create a customer via POST /api/Customer/sync
    /// 2. Query the audit logs for the created customer
    /// 3. Verify audit record exists with Action = CREATE
    /// 4. Verify audit metadata is populated (ChangedBy, RequestId)
    [Fact]
    //[Fact(DisplayName = "AUDIT001 POST Customer creates audit record")]
    public async Task AUDIT001_PostCustomer_CreatesAuditRecord()
    {
        // Arrange: Generate unique customer ID
        var customerId = $"AUDIT_TEST_001_{DateTime.UtcNow:yyyyMMddHHmmss}";

        // Act: Create customer
        var request = new CustomersDto
        {
            CRMCustomerID = customerId,
            FirstName = "Audit",
            LastName = "Create",
            Email = "audit.create@test.com",
            Phone = TestPhone
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        // Assert: Verify successful creation
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Assert: Verify audit record exists
        var audit = await GetLatestAuditRecordAsync(customerId, "CREATE");
        Assert.NotNull(audit);
        Assert.Equal("Customer", audit.EntityName);
        Assert.Equal("CREATE", audit.Action);

        // Assert: Verify audit metadata is populated
        Assert.False(string.IsNullOrWhiteSpace(audit.ChangedBy));
        Assert.False(string.IsNullOrWhiteSpace(audit.RequestId));
    }


    /// AUDIT002: Verifies that updating a customer stores both old and new values.
    /// 
    /// Workflow:
    /// 1. Create a customer
    /// 2. Update the customer with new values
    /// 3. Query the audit logs for the UPDATE record
    /// 4. Verify both OldValues and NewValues are populated
    [Fact]
   // [Fact(DisplayName = "AUDIT002 Update Customer stores old and new values")]
    public async Task AUDIT002_UpdateCustomer_StoresOldAndNewValues()
    {
        // Arrange: Create customer first
        const string customerId = "AUDIT_UPDATE_001";

        await CreateCustomerAsync(
            customerId,
            firstName: "Original",
            email: "old@test.com",
            phone: "6041111111");

        // Act: Update customer
        var update = new CustomersDto
        {
            CRMCustomerID = customerId,
            FirstName = "Updated",
            LastName = "Customer",
            Email = "updated@test.com",
            Phone = "6049999999"
        };

        var response = await UpdateCustomerAsync(customerId, update);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Assert: Verify UPDATE audit record
        var audit = await GetLatestAuditRecordAsync(customerId, "UPDATE");
        Assert.NotNull(audit);

        // Assert: Verify both old and new values are stored
        Assert.False(string.IsNullOrWhiteSpace(audit.OldValues));
        Assert.False(string.IsNullOrWhiteSpace(audit.NewValues));
    }


    /// AUDIT003: Verifies that deleting a customer generates a DELETE audit record.
    /// 
    /// Workflow:
    /// 1. Create a customer
    /// 2. Delete the customer
    /// 3. Query the audit logs for the DELETE record
    /// 4. Verify audit record exists with Action = DELETE
    [Fact]
  //  [Fact(DisplayName = "AUDIT003 Delete Customer creates delete audit")]
    public async Task AUDIT003_DeleteCustomer_CreatesDeleteAudit()
    {
        // Arrange: Create customer
        const string customerId = "AUDIT_DELETE_001";

        await CreateCustomerAsync(
            customerId,
            firstName: "Delete",
            email: "delete@test.com",
            phone: "6042222222");

        // Act: Delete customer
        var response = await DeleteCustomerAsync(customerId);
        Assert.True(response.IsSuccessStatusCode);

        // Assert: Verify DELETE audit record
        var audit = await GetLatestAuditRecordAsync(customerId, "DELETE");
        Assert.NotNull(audit);
    }

    #endregion

    #region Authorization Tests


    /// AUDIT004: Verifies that unauthorized users cannot access audit endpoints.
    /// 
    /// Workflow:
    /// 1. Create a new client without authentication
    /// 2. Attempt to access /api/AuditLogs
    /// 3. Verify request is rejected with 401 Unauthorized

    [Fact]
   // [Fact(DisplayName = "AUDIT004 Unauthorized user cannot access audit endpoint")]
    public async Task AUDIT004_UnauthorizedUser_CannotAccessAuditEndpoint()
    {
        // Arrange: Create unauthenticated client
        var unauthenticatedClient = _factory.CreateClient();

        // Act: Attempt to access audit logs
        var response = await unauthenticatedClient.GetAsync("/api/AuditLogs");

        // Assert: Should return 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }


    /// AUDIT008: Verifies that admin users can view audit logs.
    /// 
    /// Workflow:
    /// 1. Set admin authentication token
    /// 2. Request the audit logs endpoint
    /// 3. Verify access is granted (200 OK)
    [Fact]
   // [Fact(DisplayName = "AUDIT008 Admin can view audit logs")]
    public async Task AUDIT008_AdminCanViewAuditLogs()
    {
        // Act: Admin requests audit logs
        var response = await _client.GetAsync("/api/AuditLogs");

        // Assert: Should return 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Audit Metadata Tests


    /// AUDIT005: Verifies that audit records include a valid timestamp.
    /// 
    /// Workflow:
    /// 1. Retrieve any audit record from the database
    /// 2. Verify the ChangedDate property exists and is valid
    /// 3. Ensure timestamp is greater than DateTime.MinValue
    [Fact]
   // [Fact(DisplayName = "AUDIT005 Audit timestamp exists")]
    public async Task AUDIT005_AuditTimestampExists()
    {
        // Arrange: Get database context
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Act: Get first audit record
        var audit = await db.AuditLogs.FirstAsync();

        // Assert: Verify timestamp exists and is valid
        Assert.True(audit.ChangedDate > DateTime.MinValue);
    }


    /// AUDIT006: Verifies that audit records track who made the change.
    /// 
    /// Workflow:
    /// 1. Retrieve any audit record from the database
    /// 2. Verify the ChangedBy property is populated
    /// 3. Ensure username is not empty
    [Fact]
   // [Fact(DisplayName = "AUDIT006 ChangedBy is populated")]
    public async Task AUDIT006_ChangedByIsPopulated()
    {
        // Arrange: Get database context
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Act: Get first audit record
        var audit = await db.AuditLogs.FirstAsync();

        // Assert: Verify ChangedBy is populated
        Assert.False(string.IsNullOrWhiteSpace(audit.ChangedBy));
    }


    /// AUDIT007: Verifies that audit records include request correlation IDs.
    /// 
    /// Workflow:
    /// 1. Retrieve any audit record from the database
    /// 2. Verify the RequestId property is populated
    /// 3. Ensure RequestId is not empty
    [Fact]
 //   [Fact(DisplayName = "AUDIT007 RequestId stored")]
    public async Task AUDIT007_RequestIdStored()
    {
        // Arrange: Get database context
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Act: Get first audit record
        var audit = await db.AuditLogs.FirstAsync();

        // Assert: Verify RequestId is populated
        Assert.False(string.IsNullOrWhiteSpace(audit.RequestId));
    }

    #endregion
}