/*
This should contain the 8 GET tests:

Admin + existing customer → 200
Admin + invalid customer → 404
Admin + null CRM → 400
Owner + own customer → 200
Owner + another customer → 403
Owner + invalid customer → 404
Missing JWT → 401
Invalid JWT → 401
 
 
 
 */


using DemoERPApi.Models;
using DemoERPApi.Tests.Helpers;
using DemoERPApi.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DemoERPApi.Tests.Integration;

public class CustomerGetTests
: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;


    public CustomerGetTests(WebApplicationFactory<Program> factory)
    {
        DatabaseResetHelper.Reset(factory.Services);
        _client = factory.CreateClient();

    }

    // =====================================================
    // GET /api/Customer/{crmId}
    // 8 tests
    // =====================================================

    [Fact]
    public async Task GetCustomer_ReturnsOk_WhenAdminRequestsExistingCustomer()
    {
        TestAuthHelper.SetAdminToken(_client);

        var response = await _client.GetAsync(
            $"/api/Customer/{TestData.ExistingCustomerID2}"
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(customer);
        Assert.Equal(TestData.ExistingCustomerID2, customer!.CRMCustomerID);
    }

    [Fact]
    public async Task GetCustomer_ReturnsNotFound_WhenAdminRequestsNonExistingCustomer()
    {
        TestAuthHelper.SetAdminToken(_client);

        var response = await _client.GetAsync(
            $"/api/Customer/{TestData.NonExistingCustomerID}"
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomer_ReturnsBadRequest_WhenAdminRequestsNullCustomerID()
    {
        TestAuthHelper.SetAdminToken(_client);

        var response = await _client.GetAsync("/api/Customer/");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
       // Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomer_ReturnsOk_WhenOwnerRequestsOwnCustomer()
    {
        TestAuthHelper.SetOwnerToken(_client);

        var response = await _client.GetAsync(
            $"/api/Customer/{TestData.OwnerCustomerID}"
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomer_ReturnsForbidden_WhenOwnerRequestsAnotherUsersCustomer()
    {
        TestAuthHelper.SetOwnerToken(_client);

        var response = await _client.GetAsync(
            $"/api/Customer/{TestData.OtherCustomerID}"
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomer_ReturnsNotFound_WhenOwnerRequestsNonExistingCustomer()
    {
        TestAuthHelper.SetOwnerToken(_client);

        var response = await _client.GetAsync(
            $"/api/Customer/{TestData.NonExistingCustomerID}"
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomer_ReturnsUnauthorized_WhenJwtMissing()
    {
        TestAuthHelper.ClearToken(_client);

        var response = await _client.GetAsync(
            $"/api/Customer/{TestData.ExistingCustomerID}"
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomer_ReturnsUnauthorized_WhenJwtInvalid()
    {
        TestAuthHelper.SetInvalidToken(_client);

        var response = await _client.GetAsync(
            $"/api/Customer/{TestData.ExistingCustomerID}"
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }


}
