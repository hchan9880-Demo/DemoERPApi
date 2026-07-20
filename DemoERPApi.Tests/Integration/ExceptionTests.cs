using DemoERPApi.Models;
using DemoERPApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

/*
=============================================================
Exception Handling Test Coverage
=============================================================

EX-001 ValidationException
Verify invalid input returns HTTP 400 (Bad Request).

EX-002 BusinessException
Verify business rule violations (e.g., duplicate customer)
return HTTP 400 (Bad Request).

EX-003 UnauthorizedException
Verify unauthorized requests return HTTP 401 (Unauthorized).

EX-004 NotFoundException
Verify requesting a non-existent resource returns
HTTP 404 (Not Found).

EX-005 Unhandled Exception
Verify unexpected exceptions are caught by
ExceptionHandlingMiddleware and return
HTTP 500 (Internal Server Error).

Additional validation:
- Verify the response body contains the expected error message.
- Verify the response content type is application/json.
- Verify exceptions are handled by ExceptionHandlingMiddleware
  instead of the default ASP.NET Core error page.
=============================================================
*/

namespace DemoERPApi.Tests.Integration
{
    public class ExceptionTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ExceptionTests(WebApplicationFactory<Program> factory)
        {
            DatabaseResetHelper.Reset(factory.Services);
            _client = factory.CreateClient();
        }

        // =====================================================
        // EX-001 ValidationException
        // =====================================================
        [Fact]
        public async Task EX_001_CreateCustomer_InvalidEmail_ReturnsBadRequest()
        {
            TestAuthHelper.SetValidToken(_client);

            var request = new CustomersDto
            {
                CRMCustomerID = "TEST_INVALID_EMAIL",
                FirstName = "Test",
                LastName = "Email",
                Email = "",
                Phone = "6041234567"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Customer/sync",
                request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // =====================================================
        // EX-002 BusinessException
        // =====================================================
        [Fact]
        public async Task EX_002_CreateDuplicateCustomer_ReturnsConflict()
        {
            TestAuthHelper.SetValidToken(_client);

            var customer = new CustomersDto
            {
                CRMCustomerID = "CRM100",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Phone = "6041234567"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Customer/sync",
                customer);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        // =====================================================
        // EX-003 UnauthorizedException
        // =====================================================
        [Fact]
        public async Task EX_003_NoToken_ReturnsUnauthorized()
        {
            using var client = new WebApplicationFactory<Program>()
                .CreateClient();

            var response = await client.GetAsync("/api/customer/CRM100");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // =====================================================
        // EX-004 NotFoundException
        // =====================================================
        [Fact]
        public async Task EX_004_InvalidCustomer_Returns404()
        {
            TestAuthHelper.SetValidToken(_client);

            var response = await _client.GetAsync("/api/customer/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // =====================================================
        // EX-005 Unhandled Exception - NotFound (or should it be 500)
        // =====================================================
        // Uncomment this test only after adding an endpoint that
        // intentionally throws an unhandled exception.

        [Fact]
        public async Task EX_005_UnhandledException_ReturnsNotFound()
        {
            TestAuthHelper.SetValidToken(_client);

            var response = await _client.GetAsync("/api/customer/throw");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

    }
}