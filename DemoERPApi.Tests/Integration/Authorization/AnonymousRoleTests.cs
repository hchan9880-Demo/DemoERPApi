using DemoERPApi.Models;
using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace DemoERPApi.Tests.Integration.Authorization;

/*
Test Cases Covered:

AUTH-040a POST   /api/Customer/sync        Anonymous Role -> 403 Forbidden
AUTH-040b GET    /api/Customer/{id}        Anonymous Role -> 403 Forbidden
AUTH-040c PUT    /api/Customer/{id}        Anonymous Role -> 403 Forbidden
AUTH-040d DELETE /api/Customer/{id}        Anonymous Role -> 403 Forbidden
AUTH-040e PUT    /api/Customer/{id}        Anonymous Role -> 403 Forbidden
AUTH-040f DELETE /api/Customer/{id}        Anonymous Role -> 403 Forbidden
AUTH-040g POST   /api/Customer/sync        Anonymous Role -> 403 Forbidden

*/


public class AnonymousRoleTests
    : IClassFixture<CustomWebApplicationFactory>
{

    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;



    public AnonymousRoleTests(
        CustomWebApplicationFactory factory,
        ITestOutputHelper output)
    {

        _client =
            factory.CreateClient();


        _client
            .DefaultRequestHeaders
            .Authorization = null;


        _output = output;

    }





    private static CustomerDto GetValidPayload(
        string customerId)
    {

        return new CustomerDto
        {
            CRMCustomerID = customerId,
            FirstName = "Unauthorized",
            LastName = "Anonymous",
            Email = "anonymous@test.com",
            Phone = "6040000000"
        };

    }






    private async Task PrintResponse(
        HttpResponseMessage response)
    {

        var body =
            await response.Content.ReadAsStringAsync();


        _output.WriteLine(
            "==============================");


        _output.WriteLine(
            $"STATUS: {response.StatusCode}");


        _output.WriteLine(body);


        _output.WriteLine(
            "==============================");

    }






    // =====================================================
    // AUTH-040a
    // Anonymous sync customer
    // =====================================================
    [Fact]
    public async Task AUTH_040a_SyncCustomer_WithAnonymousRoleClaimToken_ReturnsForbidden()
    {

        var testId =
            "CRM_ANON_SYNC_040A";


        var dbHelper =
            new TestDatabaseHelper();


        await dbHelper.DeleteCustomer(testId);



        TestAuthHelper.SetTokenWithRole(
            _client,
            "Anonymous");



        var response =
            await _client.PostAsJsonAsync(
                "/api/Customer/sync",
                GetValidPayload(testId));



        await PrintResponse(response);



        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

    }







    // =====================================================
    // AUTH-040b
    // Anonymous GET customer
    // =====================================================
    [Fact]
    public async Task AUTH_040b_GetCustomer_WithAnonymousRoleClaimToken_ReturnsForbidden()
    {

        var testId =
            "CRM_ANON_GET_040B";


        TestAuthHelper.SetAdminToken(_client);



        await CustomerSeedHelper.SeedCustomer(
            _client,
            testId,
            _output);




        TestAuthHelper.SetTokenWithRole(
            _client,
            "Anonymous");



        var response =
            await _client.GetAsync(
                $"/api/Customer/{testId}");



        await PrintResponse(response);



        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

    }








    // =====================================================
    // AUTH-040c
    // Anonymous PUT customer
    // =====================================================
    [Fact]
    public async Task AUTH_040c_UpdateCustomer_WithAnonymousRoleClaimToken_ReturnsForbidden()
    {

        var testId =
            "CRM_ANON_PUT_040C";



        TestAuthHelper.SetAdminToken(_client);



        await CustomerSeedHelper.SeedCustomer(
            _client,
            testId,
            _output);




        TestAuthHelper.SetTokenWithRole(
            _client,
            "Anonymous");



        var payload =
            GetValidPayload(testId);



        payload.FirstName =
            "UnauthorizedUpdate";



        var response =
            await _client.PutAsJsonAsync(
                $"/api/Customer/{testId}",
                payload);



        await PrintResponse(response);



        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

    }







    // =====================================================
    // AUTH-040d
    // Anonymous DELETE customer
    // =====================================================
    [Fact]
    public async Task AUTH_040d_DeleteCustomer_WithAnonymousRoleClaimToken_ReturnsForbidden()
    {

        var testId =
            "CRM_ANON_DEL_040D";



        TestAuthHelper.SetAdminToken(_client);



        await CustomerSeedHelper.SeedCustomer(
            _client,
            testId,
            _output);



        TestAuthHelper.SetTokenWithRole(
            _client,
            "Anonymous");



        var response =
            await _client.DeleteAsync(
                $"/api/Customer/{testId}");



        await PrintResponse(response);



        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

    }







    // =====================================================
    // AUTH-040e
    // Anonymous alternative PUT
    // =====================================================
    [Fact]
    public async Task AUTH_040e_UpdateCustomerAlternative_WithAnonymousRoleClaimToken_ReturnsForbidden()
    {

        var testId =
            "CRM_ANON_PUT_040E";



        TestAuthHelper.SetAdminToken(_client);



        await CustomerSeedHelper.SeedCustomer(
            _client,
            testId,
            _output);




        TestAuthHelper.SetTokenWithRole(
            _client,
            "Anonymous");



        var response =
            await _client.PutAsJsonAsync(
                $"/api/Customer/{testId}",
                GetValidPayload(testId));



        await PrintResponse(response);



        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

    }








    // =====================================================
    // AUTH-040f
    // Anonymous alternative DELETE
    // =====================================================
    [Fact]
    public async Task AUTH_040f_DeleteCustomerAlternative_WithAnonymousRoleClaimToken_ReturnsForbidden()
    {

        var testId =
            "CRM_ANON_DEL_040F";



        TestAuthHelper.SetAdminToken(_client);



        await CustomerSeedHelper.SeedCustomer(
            _client,
            testId,
            _output);




        TestAuthHelper.SetTokenWithRole(
            _client,
            "Anonymous");



        var response =
            await _client.DeleteAsync(
                $"/api/Customer/{testId}");



        await PrintResponse(response);



        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

    }







    // =====================================================
    // AUTH-040g
    // Anonymous sync alternative
    // =====================================================
    [Fact]
    public async Task AUTH_040g_SyncCustomerAlternative_WithAnonymousRoleClaimToken_ReturnsForbidden()
    {

        var testId =
            "CRM_ANON_SYNC_040G";



        var dbHelper =
            new TestDatabaseHelper();



        await dbHelper.DeleteCustomer(testId);



        TestAuthHelper.SetTokenWithRole(
            _client,
            "Anonymous");



        var response =
            await _client.PostAsJsonAsync(
                "/api/Customer/sync",
                GetValidPayload(testId));



        await PrintResponse(response);



        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);

    }

}