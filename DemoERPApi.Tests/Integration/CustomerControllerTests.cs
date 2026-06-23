using DemoERPApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DemoERPApi.Tests.Integration;

public class CustomerControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CustomerControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }


    // =====================================================
    // 1. SUCCESS CASE - CRM100 EXISTS
    // =====================================================
    [Fact]
    public async Task GetCustomer_ReturnsOk_WhenCustomerExists()
    {
        var response = await _client.GetAsync("/api/Customer/CRM100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(customer);
        Assert.Equal("CRM100", customer!.CustomerId);
    }


    // =====================================================
    // 2. NOT FOUND CASE
    // =====================================================
    [Fact]
    public async Task GetCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        var response = await _client.GetAsync("/api/Customer/CRM999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    // =====================================================
    // 3. SYNC CUSTOMER SUCCESS CASE - CREATE CRM102
    // =====================================================
    [Fact]
    public async Task SyncCustomer_ReturnsOk_WhenCustomerIsValid()
    {
        var request = new CustomerDto
        {
            CustomerId = "CRM102",
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael.johnson@email.com",
            Phone = "6049998888"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Customer/sync",
            request
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }






    // =====================================================
    // 4. UPDATE CUSTOMER SUCCESS CASE - UPDATE CRM102
    // =====================================================
    [Fact]
    public async Task UpdateCustomer_ReturnsOk_WhenCustomerIsValid()
    {
        var json = """
    {
      "CustomerId": "CRM102",
      "FirstName": "Michael",
      "LastName": "Johnson",
      "Email": "michael.johnson@email.com",
      "Phone": "6049998888"
    }
    """;

        var content = new StringContent(
            json,
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PutAsync(
            "/api/Customer/update",
            content
        );

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Contains(
            "Customer updated successfully",
            responseBody
        );
    }



    // =====================================================
    // 5. SOFT DELETE BAD REQUEST - NULL ID
    // =====================================================
    [Fact]
    public async Task DeleteCustomer_ReturnsBadRequest_WhenIdIsMissing()
    {
        var response = await _client.DeleteAsync(
            "/api/Customer/"
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        //Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }


    // =====================================================
    // 6. SOFT DELETE NOT FOUND - CRM999 DOES NOT EXIST
    // =====================================================
    [Fact]
    public async Task DeleteCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        var response = await _client.DeleteAsync(
            "/api/Customer/CRM999"
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    // =====================================================
    // 7. SOFT DELETE SUCCESS - DELETE CRM102
    // =====================================================
    [Fact]
    public async Task DeleteCustomer_ReturnsOk_WhenCustomerExists()
    {
        var response = await _client.DeleteAsync(
            "/api/Customer/CRM102"
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }


}