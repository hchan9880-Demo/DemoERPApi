using DemoERPApi.Data;
using DemoERPApi.Models;
using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace DemoERPApi.Tests.Integration.Logging
{
    /*
    ===============================================================================
     Audit Integration Tests

     Purpose:
        Validate enterprise audit logging functionality.

     Test Coverage:

        AUDIT001
            POST Customer
            ->
            Audit record created


        AUDIT002
            PUT Customer
            ->
            OldValues stored
            ->
            NewValues stored


        AUDIT003
            DELETE Customer
            ->
            Audit Action = DELETE


        AUDIT004
            Unauthorized user
            ->
            Cannot access audit endpoint


        AUDIT005
            Audit timestamp exists


        AUDIT006
            ChangedBy populated


        AUDIT007
            RequestId stored

    ===============================================================================
    */


    public class AuditTests :
        IClassFixture<CustomWebApplicationFactory>
    {

        private readonly CustomWebApplicationFactory _factory;

        public object JwtTokenHelper { get; private set; }

        public AuditTests(
            CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }



        private HttpClient CreateAuthenticatedClient()
        {
            var client =
                _factory.CreateClient();



            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    JwtTestTokenHelper.GenerateToken());



            return client;
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

        [Fact]
        public async Task AUDIT001_PostCustomer_CreatesAuditRecord()
        {
            var client =
                CreateAuthenticatedClient();


            var customer =
                new CustomerDto
                {
                    CRMCustomerID =
                        "AUDIT_TEST_001",

                    FirstName =
                        "Audit",

                    LastName =
                        "Create",

                    Email =
                        "audit.create@test.com",

                    Phone =
                        "6041234567"
                };
            /*



            Assert.True(
                response.IsSuccessStatusCode);

            */



            var response =
                await client.PostAsJsonAsync(
                    "/api/Customer/sync",
                    customer);

            var responseBody =
    await response.Content.ReadAsStringAsync();


            Assert.True(
                response.IsSuccessStatusCode,
                $"API failed. Status={response.StatusCode}, Body={responseBody}");







            using var scope =
                _factory.Services.CreateScope();


            var db =
                scope.ServiceProvider
                .GetRequiredService<AppDbContext>();


            var audit =
                await db.AuditLogs
                .FirstOrDefaultAsync(x =>
                    x.EntityId ==
                    "AUDIT_TEST_001"
                    &&
                    x.Action == "CREATE");



            Assert.NotNull(audit);

            Assert.Equal(
                "Customer",
                audit.EntityName);
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

        [Fact]
        public async Task AUDIT002_UpdateCustomer_StoresOldAndNewValues()
        {
            var client =
                CreateAuthenticatedClient();


            var update =
                new CustomerDto
                {
                    CRMCustomerID =
                        "AUDIT_UPDATE_001",

                    FirstName =
                        "Updated",

                    LastName =
                        "Customer",

                    Email =
                        "updated@test.com",

                    Phone =
                        "6049999999"
                };


            var response =
                await client.PutAsJsonAsync(
                    "/api/Customer/AUDIT_UPDATE_001",
                    update);


            Assert.True(
                response.StatusCode ==
                HttpStatusCode.OK);



            using var scope =
                _factory.Services.CreateScope();


            var db =
                scope.ServiceProvider
                .GetRequiredService<AppDbContext>();



            var audit =
                await db.AuditLogs
                .Where(x =>
                    x.EntityId ==
                    "AUDIT_UPDATE_001"
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

        [Fact]
        public async Task AUDIT003_DeleteCustomer_CreatesDeleteAudit()
        {
            var client =
                CreateAuthenticatedClient();



            var response =
                await client.DeleteAsync(
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
                    x.EntityId ==
                    "AUDIT_DELETE_001"
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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
    }
}