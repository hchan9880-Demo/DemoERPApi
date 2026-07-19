using DemoERPApi.Models;
using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DemoERPApi.Tests.Integration.Customer;

/*
Test Cases Covered:
UPDATE-001	Duplicate customer conflict on update	    Update payload duplicates another customer's unique constraints
UPDATE-004	Admin updates any customer	                Valid update payload
UPDATE-005	Admin updates non-existent customer	        Valid payload with unknown id
UPDATE-006	QA updates assigned customer	            Valid update payload
UPDATE-007	QA updates non-existent assigned customer	Valid payload
UPDATE-008	Customer updates non-existent own record	Valid payload
UPDATE-019	Update deleted customer	                    Valid update payload for deleted customer
*/

public class CustomerUpdateTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CustomerUpdateTests(WebApplicationFactory<Program> factory)
    {
        DatabaseResetHelper.Reset(factory.Services);
        _client = factory.CreateClient();

        var helper = new TestDatabaseHelper();
        helper.ResetDatabase().GetAwaiter().GetResult();
        helper.SeedCustomers().GetAwaiter().GetResult();
    }

    // =====================================================
    // UPDATE-001: Duplicate customer conflict on update
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Customer exists?
    //      ↓ Yes
    // Update payload valid?
    //      ↓ Yes
    // Update creates duplicate?
    //      ↓ Yes
    // Return 409 Conflict
    // =====================================================
    [Fact]
    public async Task UPDATE_001_UpdatePayloadDuplicatesAnotherCustomer_ReturnsConflict()
    {
        // Arrange

        await CustomerSeedHelper.SeedCustomer(
            _client,
            "CRM100");

        await CustomerSeedHelper.SeedCustomer(
            _client,
            "CRM101");


        TestAuthHelper.SetAdminToken(_client);


        var request = new CustomersDto
        {
            // trying to rename CRM100 to CRM101
            CRMCustomerID = "CRM101",

            FirstName = "Duplicate",
            LastName = "Customer",
            Email = "duplicate@test.com",
            Phone = "6041234567"
        };


        // Act

        var response =
            await _client.PutAsJsonAsync(
                "/api/Customer/CRM100",
                request);


        // Assert

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);
    }
    // =====================================================
    // UPDATE-004: Admin updates any customer
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Customer exists?
    //      ↓ Yes
    // Update payload valid?
    //      ↓ Yes
    // Update customer
    //      ↓
    // Return 200 OK
    // =====================================================
    [Fact]
    public async Task UPDATE_004_AdminUpdatesAnyCustomer_WithValidPayload_ReturnsOk()
    {
        TestAuthHelper.SetAdminToken(_client);
        var testId = "CRM_UPD_004";
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var request = new CustomersDto
        {
            CRMCustomerID = testId,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael.updated@test.com",
            Phone = "6049998888"
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{testId}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // =====================================================
    // UPDATE-005: Admin updates non-existent customer
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Customer exists?
    //      ↓ No
    // Return 404 NotFound
    // =====================================================
    [Fact]
    public async Task UPDATE_005_AdminUpdatesNonExistentCustomer_ReturnsNotFound()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomersDto
        {
            CRMCustomerID = TestData.NonExistingCustomerID,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            Phone = "6041112222"
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{TestData.NonExistingCustomerID}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    // =====================================================
    // UPDATE-006: QA updates assigned customer
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = QA
    //      ↓
    // Customer assigned to QA?
    //      ↓ Yes
    // Update payload valid?
    //      ↓ Yes
    // Update customer
    //      ↓
    // Return 200 OK
    // =====================================================
    [Fact]
    public async Task UPDATE_006_QAUpdatesAssignedCustomer_WithValidPayload_ReturnsOk()
    {
        TestAuthHelper.SetQAToken(_client);
        var testId = "CRM_UPD_006";
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var request = new CustomersDto
        {
            CRMCustomerID = testId,
            FirstName = "QA",
            LastName = "AssignedUpdate",
            Email = "qa.updated@test.com",
            Phone = "6045551234"
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{testId}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    // =====================================================
    // UPDATE-007: QA updates non-existent assigned customer
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = QA
    //      ↓
    // Supported role
    //      ↓
    // Customer exists?
    //      ↓ No
    // Return 404 NotFound
    // =====================================================
    [Fact]
    public async Task UPDATE_007_QAUpdatesNonExistentAssignedCustomer_ReturnsNotFound()
    {
        TestAuthHelper.SetQAToken(_client);

        var request = new CustomersDto
        {
            CRMCustomerID = TestData.NonExistingCustomerID,
            FirstName = "Missing",
            LastName = "QA",
            Email = "missingqa@test.com",
            Phone = "6045550000"
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{TestData.NonExistingCustomerID}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    // =====================================================
    // UPDATE-008: Customer updates non-existent own record
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Customer
    //      ↓
    // Customer exists?
    //      ↓ No
    // Return 404 NotFound
    // =====================================================
    [Fact]
    public async Task UPDATE_008_CustomerUpdatesNonExistentOwnRecordReference_ReturnsNotFound()
    {
        TestAuthHelper.SetOwnerToken(_client);

        var request = new CustomersDto
        {
            CRMCustomerID = TestData.NonExistingCustomerID,
            FirstName = "Ghost",
            LastName = "Owner",
            Email = "ghostowner@test.com",
            Phone = "6041113333"
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{TestData.NonExistingCustomerID}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    // =====================================================
    // UPDATE-019: Update deleted customer
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Customer exists?
    //      ↓ Yes (Soft Deleted)
    // Update request received
    //      ↓
    // Update blocked
    //      ↓
    // Return 404 NotFound
    // =====================================================
    [Fact]
    public async Task UPDATE_019_UpdateDeletedCustomer_ReturnsNotFoundOrBadRequest()
    {
        TestAuthHelper.SetAdminToken(_client);
        var testId = "CRM_UPD_019";
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        // Soft-delete the customer record first via api route template
        await _client.DeleteAsync($"/api/Customer/{testId}");

        var request = new CustomersDto
        {
            CRMCustomerID = testId,
            FirstName = "Attempted",
            LastName = "UpdateOnDeleted",
            Email = "deleted_update@test.com",
            Phone = "6042221111"
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{testId}", request);

        // Soft-deleted records are typical blocks against payload mutations
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // =====================================================
    // SECURITY & VALIDATION SCHEMAS
    // =====================================================
    // =====================================================
    // SECURITY-UPDATE-001: Customer updates own customer
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Customer
    //      ↓
    // Requested customer is owner?
    //      ↓ Yes
    // Update payload valid?
    //      ↓ Yes
    // Update customer
    //      ↓
    // Return 200 OK
    // =====================================================
    [Fact]
    public async Task UpdateCustomer_ReturnsOk_WhenOwnerUpdatesOwnCustomer()
    {
        TestAuthHelper.SetOwnerToken(_client);

        var request = new CustomersDto
        {
            CRMCustomerID = TestData.OwnerCustomerID,
            FirstName = "Owner",
            LastName = "Updated",
            Email = "owner@test.com",
            Phone = "6048887777"
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{TestData.OwnerCustomerID}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    // =====================================================
    // SECURITY-UPDATE-002: Customer updates another user's customer
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Customer
    //      ↓
    // Requested customer is owner?
    //      ↓ No
    // Authorization fails
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact]
    public async Task UpdateCustomer_ReturnsForbidden_WhenOwnerUpdatesAnotherUsersCustomer()
    {
        TestAuthHelper.SetOwnerToken(_client);

        var request = new CustomersDto
        {
            CRMCustomerID = TestData.OtherCustomerID,
            FirstName = "Other",
            LastName = "User",
            Email = "other@test.com",
            Phone = "6047776666"
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{TestData.OtherCustomerID}", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // VALIDATION-UPDATE-001: Missing Customer ID
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // CustomerId supplied?
    //      ↓ No
    // Route not matched
    //      ↓
    // Return 404 NotFound
    // =====================================================
    [Fact]
    public async Task UpdateCustomer_ReturnsNotFound_WhenCustomerIDIsMissing()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomersDto
        {
            CRMCustomerID = null,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "6049998888"
        };

        var response = await _client.PutAsJsonAsync("/api/Customer/", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    // =====================================================
    // VALIDATION-UPDATE-002: Missing First Name
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // FirstName supplied?
    //      ↓ No
    // Model validation fails
    //      ↓
    // Return 400 BadRequest
    // =====================================================
    [Fact]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenFirstNameIsMissing()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomersDto
        {
            CRMCustomerID = TestData.ExistingCustomerID,
            FirstName = null,
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "6049998888"
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{TestData.ExistingCustomerID}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    // =====================================================
    // VALIDATION-UPDATE-003: Missing Email
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Email supplied?
    //      ↓ No
    // Model validation fails
    //      ↓
    // Return 400 BadRequest
    // =====================================================
    [Fact]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenEmailIsMissing()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomersDto
        {
            CRMCustomerID = TestData.ExistingCustomerID,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = null,
            Phone = "6049998888"
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{TestData.ExistingCustomerID}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    // =====================================================
    // VALIDATION-UPDATE-004: Invalid Email Format
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Email format valid?
    //      ↓ No
    // Model validation fails
    //      ↓
    // Return 400 BadRequest
    // =====================================================
    [Fact]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenEmailFormatIsInvalid()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomersDto
        {
            CRMCustomerID = TestData.ExistingCustomerID,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "abc",
            Phone = "6049998888"
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{TestData.ExistingCustomerID}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenPhoneIsInvalid()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomersDto
        {
            CRMCustomerID = TestData.ExistingCustomerID,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "555"
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{TestData.ExistingCustomerID}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    // =====================================================
    // VALIDATION-UPDATE-006: Missing Phone Number
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Phone supplied?
    //      ↓ No
    // Model validation fails
    //      ↓
    // Return 400 BadRequest
    // =====================================================
    [Fact]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenPhoneIsNull()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomersDto
        {
            CRMCustomerID = TestData.ExistingCustomerID,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = null
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{TestData.ExistingCustomerID}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    // =====================================================
    // SECURITY-UPDATE-003: Missing JWT token
    //
    // Workflow:
    // JWT supplied?
    //      ↓ No
    // Authentication fails
    //      ↓
    // Return 401 Unauthorized
    // =====================================================
    [Fact]
    public async Task UpdateCustomer_ReturnsUnauthorized_WhenJwtMissing()
    {
        TestAuthHelper.ClearToken(_client);

        var request = new CustomersDto
        {
            CRMCustomerID = TestData.ExistingCustomerID,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "6049998888"
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{TestData.ExistingCustomerID}", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    // =====================================================
    // SECURITY-UPDATE-004: Invalid JWT token
    //
    // Workflow:
    // JWT supplied
    //      ↓
    // JWT validation fails
    //      ↓
    // Authentication fails
    //      ↓
    // Return 401 Unauthorized
    // =====================================================
    [Fact]
    public async Task UpdateCustomer_ReturnsUnauthorized_WhenJwtInvalid()
    {
        TestAuthHelper.SetInvalidToken(_client);

        var request = new CustomersDto
        {
            CRMCustomerID = TestData.ExistingCustomerID,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "6049998888"
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{TestData.ExistingCustomerID}", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}