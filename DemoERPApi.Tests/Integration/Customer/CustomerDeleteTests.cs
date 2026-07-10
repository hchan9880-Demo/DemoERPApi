using DemoERPApi.Models;
using DemoERPApi.Tests.Helpers;
using DemoERPApi.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DemoERPApi.Tests.Integration.Customer;

/*
Test Cases Covered:
DELETE-001    Admin deletes existing customer             Happy Path
DELETE-002    Admin deletes with invalid/null customer id Validation
DELETE-003    Admin deletes non-existent customer         Not Found
DELETE-004    QA deletes assigned customer                Happy Path
DELETE-005    QA deletes invalid customer id              Validation
DELETE-006    QA deletes non-existent assigned customer   Not Found
DELETE-007    Customer deletes own account if permitted   Business Rule
DELETE-008    Soft delete sets IsDeleted                  Data Integrity
DELETE-009    Deleted customer excluded from normal reads Data Integrity
DELETE-010    Delete already deleted customer             Business Rule
*/

public class CustomerDeleteTests : IClassFixture<WebApplicationFactory<Program>>
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
    // DELETE-001: Admin deletes existing customer
    // =====================================================
    [Fact]
    public async Task DELETE_001_AdminDeletesExistingCustomer_ReturnsOk()
    {
        TestAuthHelper.SetAdminToken(_client);
        var testId = "CRM_TEST_DEL_001";

        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var response = await _client.DeleteAsync($"/api/Customer/{testId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // =====================================================
    // DELETE-002: Admin deletes with invalid/null customer id
    // =====================================================
    [Fact]
    public async Task DELETE_002_AdminDeletesWithNullCustomerId_ReturnsNotFound()
    {
        TestAuthHelper.SetAdminToken(_client);

        var response = await _client.DeleteAsync("/api/Customer/");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // =====================================================
    // DELETE-003: Admin deletes non-existent customer
    // =====================================================
    [Fact]
    public async Task DELETE_003_AdminDeletesNonExistentCustomer_ReturnsNotFound()
    {
        TestAuthHelper.SetAdminToken(_client);

        var response = await _client.DeleteAsync($"/api/Customer/{TestData.NonExistingCustomerID}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // =====================================================
    // DELETE-004: QA deletes assigned customer
    // =====================================================
    [Fact]
    public async Task DELETE_004_QADeletesAssignedCustomer_ReturnsOk()
    {
        TestAuthHelper.SetQAToken(_client);
        var testId = "CRM_TEST_DEL_004";

        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var response = await _client.DeleteAsync($"/api/Customer/{testId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // =====================================================
    // DELETE-005: QA deletes invalid customer id
    // =====================================================
    [Fact]
    public async Task DELETE_005_QADeletesInvalidCustomerId_ReturnsNotFound()
    {
        TestAuthHelper.SetQAToken(_client);

        var response = await _client.DeleteAsync("/api/Customer/");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // =====================================================
    // DELETE-006: QA deletes non-existent assigned customer
    // =====================================================
    [Fact]
    public async Task DELETE_006_QADeletesNonExistentAssignedCustomer_ReturnsNotFound()
    {
        TestAuthHelper.SetQAToken(_client);

        var response = await _client.DeleteAsync($"/api/Customer/{TestData.NonExistingCustomerID}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // =====================================================
    // DELETE-007: Customer deletes own account if permitted
    // =====================================================
    [Fact]
    public async Task DELETE_007_CustomerDeletesOwnAccountIfPermitted_ReturnsForbidden()
    {
        TestAuthHelper.SetOwnerToken(_client);
        var testId = "CRM_TEST_DEL_007";

        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var response = await _client.DeleteAsync($"/api/Customer/{testId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // =====================================================
    // DELETE-008: Soft delete sets IsDeleted
    // =====================================================
    [Fact]
    public async Task DELETE_008_SoftDeleteSetsIsDeleted_InDatabase()
    {
        TestAuthHelper.SetAdminToken(_client);
        var testId = "CRM_TEST_DEL_008";

        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var response = await _client.DeleteAsync($"/api/Customer/{testId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new SqlCommand("SELECT IsDeleted FROM Customers WHERE CRMCustomerID = @id", conn);
        cmd.Parameters.AddWithValue("@id", testId);

        var isDeletedValue = await cmd.ExecuteScalarAsync();
        Assert.NotNull(isDeletedValue);
        Assert.True(Convert.ToBoolean(isDeletedValue));
    }

    // =====================================================
    // DELETE-009: Deleted customer excluded from normal reads
    // =====================================================
    [Fact]
    public async Task DELETE_009_DeletedCustomer_ExcludedFromNormalReads()
    {
        TestAuthHelper.SetAdminToken(_client);
        var testId = "CRM_TEST_DEL_009";

        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var deleteResponse = await _client.DeleteAsync($"/api/Customer/{testId}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/Customer/{testId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    // =====================================================
    // DELETE-010: Delete already deleted customer
    // =====================================================
    [Fact]
    public async Task DELETE_010_DeleteAlreadyDeletedCustomer_ReturnsNotFound()
    {
        TestAuthHelper.SetAdminToken(_client);
        var testId = "CRM_TEST_DEL_010";

        await CustomerSeedHelper.SeedCustomer(_client, testId);

        await _client.DeleteAsync($"/api/Customer/{testId}");
        var duplicateDeleteResponse = await _client.DeleteAsync($"/api/Customer/{testId}");

        Assert.Equal(HttpStatusCode.NotFound, duplicateDeleteResponse.StatusCode);
    }

    // =====================================================
    // SECURITY AUTHENTICATION CHECKS
    // =====================================================
    [Fact]
    public async Task DeleteCustomer_ReturnsUnauthorized_WhenJwtMissing()
    {
        TestAuthHelper.ClearToken(_client);

        var response = await _client.DeleteAsync($"/api/Customer/{TestData.ExistingCustomerID}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCustomer_ReturnsUnauthorized_WhenJwtInvalid()
    {
        TestAuthHelper.SetInvalidToken(_client);

        var response = await _client.DeleteAsync($"/api/Customer/{TestData.ExistingCustomerID}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}