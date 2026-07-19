using DemoERPApi.Data;
using DemoERPApi.Models;
using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using static DemoERPApi.Tests.Helpers.TestData;


namespace DemoERPApi.Tests.Integration.Logging;

/*
===============================================================================

 Audit Integration Tests

 Purpose:
    Validate enterprise audit logging functionality.

 Test Coverage:

    AUDIT001
        POST Customer
        Verify:
            Audit record created
            Action = CREATE


    AUDIT002
        PUT Customer
        Verify:
            OldValues stored
            NewValues stored


    AUDIT003
        DELETE Customer
        Verify:
            Audit Action = DELETE


    AUDIT004
        Unauthorized User
        Verify:
            Cannot access audit endpoint


    AUDIT005
        Verify:
            Audit timestamp exists


    AUDIT006
        Verify:
            ChangedBy populated


    AUDIT007
        Verify:
            RequestId stored

===============================================================================
*/


public class AuditTests :
    IClassFixture<CustomWebApplicationFactory>
{

    private readonly CustomWebApplicationFactory _factory;

    private readonly HttpClient Client;



    public AuditTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;

        Client = factory.CreateClient();

        TestAuthHelper.SetAdminToken(Client);
    }





    /*
    ===========================================================================
    AUDIT001

    POST Customer

    Verify:
        Audit record created
        Action = CREATE

    ===========================================================================
    */

    [Fact(DisplayName = "AUDIT001 POST Customer creates audit record")]
    public async Task AUDIT001_PostCustomer_CreatesAuditRecord()
    {



        var customerIdRandom =
    $"AUDIT_TEST_001_{DateTime.UtcNow:yyyyMMddHHmmss}";


        var request = new CustomersDto
        {
            CRMCustomerID = customerIdRandom,
            FirstName = "Audit",
            LastName = "Create",
            Email = "audit.create@test.com",
            Phone = "6041234567"
        };





        /*
        var request = new CustomersDto
        {
            CRMCustomerID = "AUDIT_TEST_001a",
            FirstName = "Audit",
            LastName = "Create",
            Email = "audit.create@test.com",
            Phone = "6041234567"
        };
        */

        var response =
            await Client.PostAsJsonAsync(
                "/api/Customer/sync",
                request);



        var body =
            await response.Content.ReadAsStringAsync();



        Console.WriteLine(
            $"Status: {response.StatusCode}");

        Console.WriteLine(
            $"Response: {body}");



        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);



        using var scope =
            _factory.Services.CreateScope();



        var db =
            scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var audit =
    await db.AuditLogs
    .FirstOrDefaultAsync(x =>
        x.EntityId == customerIdRandom
        &&
        x.Action == "CREATE");

        Assert.NotNull(audit);



        Assert.Equal(
            "Customer",
            audit.EntityName);



        Assert.Equal(
            "CREATE",
            audit.Action);



        Assert.False(
            string.IsNullOrWhiteSpace(
                audit.ChangedBy));



        Assert.False(
            string.IsNullOrWhiteSpace(
                audit.RequestId));
    }






    /*
    ===========================================================================
    AUDIT002

    PUT Customer

    Verify:
        OldValues stored
        NewValues stored

    ===========================================================================
    */


    [Fact(DisplayName = "AUDIT002 Update Customer stores old and new values")]
    public async Task AUDIT002_UpdateCustomer_StoresOldAndNewValues()
    {

        // Create customer first

        await Client.PostAsJsonAsync(
            "/api/Customer/sync",
            new CustomersDto
            {
                CRMCustomerID = "AUDIT_UPDATE_001",
                FirstName = "Original",
                LastName = "Customer",
                Email = "old@test.com",
                Phone = "6041111111"
            });



        var update = new CustomersDto
        {
            CRMCustomerID = "AUDIT_UPDATE_001",
            FirstName = "Updated",
            LastName = "Customer",
            Email = "updated@test.com",
            Phone = "6049999999"
        };



        var response =
            await Client.PutAsJsonAsync(
                "/api/Customer/AUDIT_UPDATE_001",
                update);



        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);



        using var scope =
            _factory.Services.CreateScope();



        var db =
            scope.ServiceProvider
            .GetRequiredService<AppDbContext>();



        var audit =
            await db.AuditLogs
            .Where(x =>
                x.EntityId == "AUDIT_UPDATE_001"
                &&
                x.Action == "UPDATE")
            .OrderByDescending(x =>
                x.ChangedDate)
            .FirstOrDefaultAsync();



        Assert.NotNull(audit);



        Assert.False(
            string.IsNullOrWhiteSpace(
                audit.OldValues));



        Assert.False(
            string.IsNullOrWhiteSpace(
                audit.NewValues));
    }






    /*
    ===========================================================================
    AUDIT003

    DELETE Customer

    Verify:
        Audit Action = DELETE

    ===========================================================================
    */


    [Fact(DisplayName = "AUDIT003 Delete Customer creates delete audit")]
    public async Task AUDIT003_DeleteCustomer_CreatesDeleteAudit()
    {


        await Client.PostAsJsonAsync(
            "/api/Customer/sync",
            new CustomersDto
            {
                CRMCustomerID = "AUDIT_DELETE_001",
                FirstName = "Delete",
                LastName = "Customer",
                Email = "delete@test.com",
                Phone = "6042222222"
            });



        var response =
            await Client.DeleteAsync(
                "/api/Customer/AUDIT_DELETE_001");



        Assert.True(
            response.IsSuccessStatusCode);



        using var scope =
            _factory.Services.CreateScope();



        var db =
            scope.ServiceProvider
            .GetRequiredService<AppDbContext>();



        var audit =
            await db.AuditLogs
            .FirstOrDefaultAsync(x =>
                x.EntityId == "AUDIT_DELETE_001"
                &&
                x.Action == "DELETE");



        Assert.NotNull(audit);
    }






    /*
    ===========================================================================
    AUDIT004

    Unauthorized User

    Verify:
        Cannot access audit endpoint

    ===========================================================================
    */

    [Fact(DisplayName = "AUDIT004_UnauthorizedUser_CannotAccessAuditEndpoint")]
    public async Task AUDIT004_UnauthorizedUser_CannotAccessAuditEndpoint()
    {
        var client =
            _factory.CreateClient();


        var response =
            await client.GetAsync(
                "/api/AuditLogs");


        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }







    /*
    ===========================================================================
    AUDIT005

    Verify timestamp exists

    ===========================================================================
    */


    [Fact(DisplayName = "AUDIT005 Audit timestamp exists")]
    public async Task AUDIT005_AuditTimestampExists()
    {

        using var scope =
            _factory.Services.CreateScope();



        var db =
            scope.ServiceProvider
            .GetRequiredService<AppDbContext>();



        var audit =
            await db.AuditLogs
            .FirstAsync();



        Assert.True(
            audit.ChangedDate >
            DateTime.MinValue);
    }






    /*
    ===========================================================================
    AUDIT006

    Verify ChangedBy populated

    ===========================================================================
    */


    [Fact(DisplayName = "AUDIT006 ChangedBy is populated")]
    public async Task AUDIT006_ChangedByIsPopulated()
    {

        using var scope =
            _factory.Services.CreateScope();



        var db =
            scope.ServiceProvider
            .GetRequiredService<AppDbContext>();



        var audit =
            await db.AuditLogs
            .FirstAsync();



        Assert.False(
            string.IsNullOrWhiteSpace(
                audit.ChangedBy));
    }






    /*
    ===========================================================================
    AUDIT007

    Verify RequestId stored

    ===========================================================================
    */


    [Fact(DisplayName = "AUDIT007 RequestId stored")]
    public async Task AUDIT007_RequestIdStored()
    {

        using var scope =
            _factory.Services.CreateScope();



        var db =
            scope.ServiceProvider
            .GetRequiredService<AppDbContext>();



        var audit =
            await db.AuditLogs
            .FirstAsync();



        Assert.False(
            string.IsNullOrWhiteSpace(
                audit.RequestId));
    }




    [Fact(DisplayName = "AUDIT008_AdminCanViewAuditLogs")]
    public async Task AUDIT008_AdminCanViewAuditLogs()
    {

        var response =
            await Client.GetAsync(
                "/api/AuditLogs");


        var body =
            await response.Content.ReadAsStringAsync();


        Console.WriteLine(body);


        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

    }
}