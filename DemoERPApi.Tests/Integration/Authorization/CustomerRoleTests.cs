using DemoERPApi.Models;
using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using DemoERPApi.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace DemoERPApi.Tests.Integration.Authorization;

/*
Test Cases Covered:
AUTH-014	/api/Customer/{id}	GET	    Customer -> Retrieves another customer's record      -> 403 Forbidden
AUTH-029	/api/Customer/{id}	PUT	    Customer -> Updates another customer's profile       -> 403 Forbidden
AUTH-032	/api/Customer/{id}	DELETE	Customer -> Deletes another customer's account       -> 403 Forbidden
AUTH-050	/api/Customer/sync	POST	Customer -> Attempts to create a new customer record -> 403 Forbidden
*/

public class CustomerRoleTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private const string OTHER_CUSTOMER_ID = "CRM_OTHER_999";

    public CustomerRoleTests(WebApplicationFactory<Program> factory)
    {
        DatabaseResetHelper.Reset(factory.Services);
        _client = factory.CreateClient();
    }

    // ===================================================================================
    // AUTH-014: Customer attempts to retrieve another customer
    // ===================================================================================
    [Fact]
    public async Task AUTH_014_CustomerRetrievesAnotherCustomer_ReturnsForbidden()
    {
        // Arrange
        await CustomerSeedHelper.SeedCustomer(_client, OTHER_CUSTOMER_ID);
        TestAuthHelper.SetOwnerToken(_client); // Authenticated as a standard 'Customer' token

        // Act
        var response = await _client.GetAsync($"/api/Customer/{OTHER_CUSTOMER_ID}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ===================================================================================
    // AUTH-029: Customer updates another customer's profile
    // ===================================================================================
    [Fact]
    public async Task AUTH_029_CustomerUpdatesAnotherCustomerProfile_ReturnsForbidden()
    {
        // Arrange
        await CustomerSeedHelper.SeedCustomer(_client, OTHER_CUSTOMER_ID);
        TestAuthHelper.SetOwnerToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = OTHER_CUSTOMER_ID,
            FirstName = "Malicious",
            LastName = "Modification",
            Email = "hacked@test.com",
            Phone = "6040000000"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Customer/{OTHER_CUSTOMER_ID}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ===================================================================================
    // AUTH-032: Customer deletes another customer's account
    // ===================================================================================
    [Fact]
    public async Task AUTH_032_CustomerDeletesAnotherCustomerAccount_ReturnsForbidden()
    {
        // Arrange
        await CustomerSeedHelper.SeedCustomer(_client, OTHER_CUSTOMER_ID);
        TestAuthHelper.SetOwnerToken(_client);

        // Act
        var response = await _client.DeleteAsync($"/api/Customer/{OTHER_CUSTOMER_ID}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ===================================================================================
    // AUTH-050: Customer attempts to create customer via sync endpoint
    // ===================================================================================
    [Fact]
    public async Task AUTH_050_CustomerAttemptsToCreateCustomer_ReturnsForbidden()
    {
        // Arrange
        var newCustomerId = "CRM_NEW_888";
        var dbHelper = new TestDatabaseHelper();
        await dbHelper.DeleteCustomer(newCustomerId);

        TestAuthHelper.SetOwnerToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = newCustomerId,
            FirstName = "Unauthorized",
            LastName = "Creation",
            Email = "noaccess@test.com",
            Phone = "6041112222"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}