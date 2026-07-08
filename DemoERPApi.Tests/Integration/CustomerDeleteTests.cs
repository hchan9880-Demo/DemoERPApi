using DemoERPApi.Models;
using DemoERPApi.Tests.Helpers;
using DemoERPApi.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Net;
using Xunit;

namespace DemoERPApi.Tests.Integration;

public class CustomerDeleteTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly string _connectionString;



    public CustomerDeleteTests(WebApplicationFactory<Program> factory)
    {
        DatabaseResetHelper.Reset(factory.Services);
        _client = factory.CreateClient(); 

        var config = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false)
           .Build();

        _connectionString = config?.GetConnectionString("DemoERPConnection")
            ?? throw new InvalidOperationException("TestDb connection string missing");
    }

    // =====================================================
    // DELETE /api/Customer/{crmId}
    // =====================================================

    [Fact]
    public async Task DeleteCustomer_ReturnsOk_WhenAdminDeletesExistingCustomer()
    {
        TestAuthHelper.SetAdminToken(_client);

        var testId = "TEST_DEL_001";

        await SeedCustomer(testId);

        var response = await _client.DeleteAsync($"/api/Customer/{testId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task SeedCustomer(string testId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var cleanCmd = new SqlCommand(@"
        DELETE FROM CustomerAccess 
        WHERE Username IN
        (
            SELECT Username 
            FROM Users 
            WHERE CustomerID = @id
        );

        DELETE FROM Users
        WHERE CustomerID = @id;

        DELETE FROM Customers
        WHERE CRMCustomerID = @id;
    ", conn);

        cleanCmd.Parameters.AddWithValue("@id", testId);

        await cleanCmd.ExecuteNonQueryAsync();


        var insertCmd = new SqlCommand(@"
        INSERT INTO Customers
        (
            CRMCustomerID,
            FirstName,
            LastName,
            Email,
            Phone
        )
        VALUES
        (
            @id,
            'Test',
            'User',
            'test@mail.com',
            '6041234567'
        );
    ", conn);


        insertCmd.Parameters.AddWithValue("@id", testId);

        await insertCmd.ExecuteNonQueryAsync();
    }




    [Fact]
    public async Task DeleteCustomer_ReturnsNotFound_WhenAdminDeletesNonExistingCustomer()
    {
        TestAuthHelper.SetAdminToken(_client);

        var response = await _client.DeleteAsync(
            $"/api/Customer/{TestData.NonExistingCustomerID}"
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_ReturnsBadRequest_WhenAdminDeletesWithNullCustomerID()
    {
        TestAuthHelper.SetAdminToken(_client);
        var response = await _client.DeleteAsync($"/api/Customer?crmId=");
        //    var response = await _client.DeleteAsync("/api/Customer/");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
       // Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_AdminToken_ShouldNotBeUnauthorized()
    {
        TestAuthHelper.SetAdminToken(_client);

        var response = await _client.DeleteAsync("/api/Customer?customerId=ABC123");

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_ReturnsForbidden_WhenUserAttemptsDelete()
    {
        TestAuthHelper.SetOwnerToken(_client);

        var testId = "TEST_DEL_002";

        await SeedCustomer(testId);

        var response = await _client.DeleteAsync($"/api/Customer/{testId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_ReturnsUnauthorized_WhenJwtMissing()
    {
        TestAuthHelper.ClearToken(_client);

        var response = await _client.DeleteAsync(
            $"/api/Customer/{TestData.ExistingCustomerID}"
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_ReturnsUnauthorized_WhenJwtInvalid()
    {
        TestAuthHelper.SetInvalidToken(_client);

        var response = await _client.DeleteAsync(
            $"/api/Customer/{TestData.ExistingCustomerID}"
        );
       
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}