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
AUTH-012	/api/Customer/{id}	GET	    QA -> Retrieves another QA user's customer       -> 403 Forbidden
AUTH-013	/api/Customer/{id}	GET	    QA -> Retrieves customer outside assignment scope -> 403 Forbidden
AUTH-027	/api/Customer/{id}	PUT	    QA -> Updates another QA user's customer         -> 403 Forbidden
AUTH-028	/api/Customer/{id}	PUT	    QA -> Updates customer outside assignment scope   -> 403 Forbidden
AUTH-030	/api/Customer/{id}	DELETE	QA -> Deletes another QA user's customer         -> 403 Forbidden
AUTH-031	/api/Customer/{id}	DELETE	QA -> Deletes customer outside assignment scope   -> 403 Forbidden
AUTH-048	/api/Customer/sync	POST	QA -> Creates customer outside assigned scope    -> 403 Forbidden
*/

public class QaRoleTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private const string OTHER_QA_CUSTOMER_ID = "CRM_QA_OTHER_101";
    private const string OUT_OF_SCOPE_CUSTOMER_ID = "CRM_OUT_OF_SCOPE_202";

    public QaRoleTests(WebApplicationFactory<Program> factory)
    {
        DatabaseResetHelper.Reset(factory.Services);
        _client = factory.CreateClient();
    }

    private static CustomerDto GetValidPayload(string crmId) => new()
    {
        CRMCustomerID = crmId,
        FirstName = "Scoped",
        LastName = "QA_Test",
        Email = $"qa_test_{crmId}@mail.com",
        Phone = "6045550199"
    };

    // ===================================================================================
    // GET /api/Customer/{id} - ASSIGNMENT SCOPE CHECKS
    // ===================================================================================

    [Fact]
    public async Task AUTH_012_GetCustomer_AssignedToDifferentQA_ReturnsForbidden()
    {
        // Arrange
        await CustomerSeedHelper.SeedCustomer(_client, OTHER_QA_CUSTOMER_ID);
        TestAuthHelper.SetQAToken(_client); // Requests token for the active QA user context

        // Act
        var response = await _client.GetAsync($"/api/Customer/{OTHER_QA_CUSTOMER_ID}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AUTH_013_GetCustomer_OutsideAssignmentScope_ReturnsForbidden()
    {
        // Arrange
        await CustomerSeedHelper.SeedCustomer(_client, OUT_OF_SCOPE_CUSTOMER_ID);
        TestAuthHelper.SetQAToken(_client);

        // Act
        var response = await _client.GetAsync($"/api/Customer/{OUT_OF_SCOPE_CUSTOMER_ID}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ===================================================================================
    // PUT /api/Customer/{id} - ASSIGNMENT SCOPE CHECKS
    // ===================================================================================

    [Fact]
    public async Task AUTH_027_UpdateCustomer_AssignedToDifferentQA_ReturnsForbidden()
    {
        // Arrange
        await CustomerSeedHelper.SeedCustomer(_client, OTHER_QA_CUSTOMER_ID);
        TestAuthHelper.SetQAToken(_client);

        var payload = GetValidPayload(OTHER_QA_CUSTOMER_ID);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Customer/{OTHER_QA_CUSTOMER_ID}", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AUTH_028_UpdateCustomer_OutsideAssignmentScope_ReturnsForbidden()
    {
        // Arrange
        await CustomerSeedHelper.SeedCustomer(_client, OUT_OF_SCOPE_CUSTOMER_ID);
        TestAuthHelper.SetQAToken(_client);

        var payload = GetValidPayload(OUT_OF_SCOPE_CUSTOMER_ID);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Customer/{OUT_OF_SCOPE_CUSTOMER_ID}", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ===================================================================================
    // DELETE /api/Customer/{id} - ASSIGNMENT SCOPE CHECKS
    // ===================================================================================

    [Fact]
    public async Task AUTH_030_DeleteCustomer_AssignedToDifferentQA_ReturnsForbidden()
    {
        // Arrange
        await CustomerSeedHelper.SeedCustomer(_client, OTHER_QA_CUSTOMER_ID);
        TestAuthHelper.SetQAToken(_client);

        // Act
        var response = await _client.DeleteAsync($"/api/Customer/{OTHER_QA_CUSTOMER_ID}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AUTH_031_DeleteCustomer_OutsideAssignmentScope_ReturnsForbidden()
    {
        // Arrange
        await CustomerSeedHelper.SeedCustomer(_client, OUT_OF_SCOPE_CUSTOMER_ID);
        TestAuthHelper.SetQAToken(_client);

        // Act
        var response = await _client.DeleteAsync($"/api/Customer/{OUT_OF_SCOPE_CUSTOMER_ID}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ===================================================================================
    // POST /api/Customer/sync - SCOPE CHECKS
    // ===================================================================================

    [Fact]
    public async Task AUTH_048_SyncCustomer_OutsideAssignedScope_ReturnsForbidden()
    {
        // Arrange
        var dbHelper = new TestDatabaseHelper();
        await dbHelper.DeleteCustomer(OUT_OF_SCOPE_CUSTOMER_ID);

        TestAuthHelper.SetQAToken(_client);
        var payload = GetValidPayload(OUT_OF_SCOPE_CUSTOMER_ID);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}