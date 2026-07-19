using DemoERPApi.Models;
using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;
using static System.Net.Mime.MediaTypeNames;

namespace DemoERPApi.Tests.Integration.Authorization;

// Test Cases Covered:
// AUTH-011a: Admin retrieves customer owned by another role
// AUTH-011b: Admin updates another user's customer
// AUTH-011c: Admin deletes another user's customer
// AUTH-011d: Admin sync customer
// AUTH-011e: Admin retrieves own customer record
// AUTH-011f: Admin updates own customer
// AUTH-011g: Admin deletes own customer
/*
 Added workflow explanation for each test cases in this format:

JWT valid
      ↓
Role = <Admin | QA | Customer>
      ↓
Authorization Policy
      ↓
Resource within permitted scope?
      ↓
Yes / No
      ↓
Business Operation (if allowed)
      ↓
Return 200 OK / 403 Forbidden
 
*/

public class AdminRoleTests
    : IClassFixture<CustomWebApplicationFactory>
{

    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;


    public AdminRoleTests(
        CustomWebApplicationFactory factory,
        ITestOutputHelper output)
    {
        _client = factory.CreateClient();

        // Prevent JWT leakage between tests
        _client.DefaultRequestHeaders.Authorization = null;

        _output = output;
    }



    private static CustomersDto GetValidPayload(string customerId)
    {
        return new CustomersDto
        {
            CRMCustomerID = customerId,
            FirstName = "Admin",
            LastName = "User Data",
            Email = "admin.test@test.com",
            Phone = "6045551212"
        };
    }



    // ===================================================================================
    // AUTH-011a: Admin retrieves customer owned by another role
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Customer exists?
    //      ↓ Yes
    // Admin has full access?
    //      ↓ Yes
    // Retrieve customer
    //      ↓
    // Return 200 OK
    // ===================================================================================
    [Fact]
    public async Task AUTH_011a_AdminRetrievesCustomerOwnedByAnotherRole_ReturnsOk()
    {

        var testId =
            $"CRM_ADMIN_GET_011A_{Guid.NewGuid():N}";


        TestAuthHelper.SetAdminToken(_client);


        _output.WriteLine(
            "========== AUTH HEADER BEFORE SEED ==========");

        _output.WriteLine(
            _client.DefaultRequestHeaders.Authorization?.ToString());


        await CustomerSeedHelper.SeedCustomer(
            _client,
            testId,
            _output);



        _output.WriteLine(
            "========== AUTH HEADER BEFORE GET ==========");

        _output.WriteLine(
            _client.DefaultRequestHeaders.Authorization?.ToString());



        var response =
            await _client.GetAsync(
                $"/api/Customer/{testId}");



        var body =
            await response.Content.ReadAsStringAsync();


        _output.WriteLine(
            $"GET STATUS = {response.StatusCode}");

        _output.WriteLine(body);



        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);



        var customer =
            await response.Content.ReadFromJsonAsync<CustomersDto>();


        Assert.NotNull(customer);


        Assert.Equal(
            testId,
            customer!.CRMCustomerID);

    }

    // ===================================================================================
    // AUTH-011b: Admin updates another user's customer
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Customer exists?
    //      ↓ Yes
    // Admin has full access?
    //      ↓ Yes
    // Update payload valid?
    //      ↓ Yes
    // Update customer
    //      ↓
    // Return 200 OK
    // ===================================================================================
    [Fact]
    public async Task AUTH_011b_AdminUpdatesAnotherCustomerProfile_ReturnsOk()
    {

        var testId =
            "CRM_ADMIN_PUT_011B";


        TestAuthHelper.SetAdminToken(_client);



        await CustomerSeedHelper.SeedCustomer(
            _client,
            testId,
            _output);



        var payload =
            GetValidPayload(testId);


        payload.FirstName =
            "AdminModifiedExternal";



        var response =
            await _client.PutAsJsonAsync(
                $"/api/Customer/{testId}",
                payload);



        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

    }

    // ===================================================================================
    // AUTH-011c: Admin deletes another user's customer
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Customer exists?
    //      ↓ Yes
    // Admin has full access?
    //      ↓ Yes
    // Soft delete customer
    //      ↓
    // Return 200 OK
    // ===================================================================================
    [Fact]
    public async Task AUTH_011c_AdminDeletesAnotherCustomerAccount_ReturnsOk()
    {

        var testId =
            "CRM_ADMIN_DEL_011C";


        TestAuthHelper.SetAdminToken(_client);



        await CustomerSeedHelper.SeedCustomer(
            _client,
            testId,
            _output);



        var response =
            await _client.DeleteAsync(
                $"/api/Customer/{testId}");



        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

    }

    // ===================================================================================
    // AUTH-011d: Admin sync customer
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Request payload valid?
    //      ↓ Yes
    // Customer already exists?
    //      ↓ No
    // Create customer
    //      ↓
    // Return 200 OK
    // ===================================================================================
    [Fact]
    public async Task AUTH_011d_AdminAttemptsToSyncCustomer_ReturnsOk()
    {
        var testId =
            $"CRM_ADMIN_SYNC_{Guid.NewGuid()}";


      //  TestDatabaseHelper.DeleteCustomer(testId);


        TestAuthHelper.SetAdminToken(_client);


        var response =
            await _client.PostAsJsonAsync(
                "/api/Customer/sync",
                GetValidPayload(testId));


        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    // ===================================================================================
    // AUTH-011e: Admin retrieves own customer record
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Customer exists?
    //      ↓ Yes
    // Own customer record
    //      ↓
    // Retrieve customer
    //      ↓
    // Return 200 OK
    // ===================================================================================
    [Fact]
    public async Task AUTH_011e_AdminRetrievesOwnCustomerRecord_ReturnsOk()
    {

        var testId =
            "CRM_ADMIN_GET_011E";


        TestAuthHelper.SetAdminToken(_client);



        await CustomerSeedHelper.SeedCustomer(
            _client,
            testId,
            _output);



        var response =
            await _client.GetAsync(
                $"/api/Customer/{testId}");



        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

    }

    // ===================================================================================
    // AUTH-011f: Admin updates own customer
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Customer exists?
    //      ↓ Yes
    // Own customer record
    //      ↓
    // Update payload valid?
    //      ↓ Yes
    // Update customer
    //      ↓
    // Return 200 OK
    // ===================================================================================
    [Fact]
    public async Task AUTH_011f_AdminUpdatesOwnCustomerProfile_ReturnsOk()
    {

        var testId =
            "CRM_ADMIN_PUT_011F";


        TestAuthHelper.SetAdminToken(_client);



        await CustomerSeedHelper.SeedCustomer(
            _client,
            testId,
            _output);



        var payload =
            GetValidPayload(testId);



        payload.FirstName =
            "AdminSelfUpdated";



        var response =
            await _client.PutAsJsonAsync(
                $"/api/Customer/{testId}",
                payload);



        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

    }


    // ===================================================================================
    // AUTH-011g: Admin deletes own customer
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Customer exists?
    //      ↓ Yes
    // Own customer record
    //      ↓
    // Soft delete customer
    //      ↓
    // Return 200 OK
    // ===================================================================================
    [Fact]
    public async Task AUTH_011g_AdminDeletesOwnCustomerAccount_ReturnsOk()
    {

        var testId =
            "CRM_ADMIN_DEL_011G";


        TestAuthHelper.SetAdminToken(_client);



        await CustomerSeedHelper.SeedCustomer(
            _client,
            testId,
            _output);



        var response =
            await _client.DeleteAsync(
                $"/api/Customer/{testId}");



        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

    }

}