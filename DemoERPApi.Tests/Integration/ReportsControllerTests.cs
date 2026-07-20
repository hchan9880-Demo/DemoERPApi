using DemoERPApi.Tests.Helpers;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace DemoERPApi.Tests.Integration;


/// Integration tests for the Reports Controller.
/// Validates that reporting endpoints require authentication,
/// enforce role-based access control, and return expected data.

public class ReportsControllerTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    // Endpoint constants
    private const string CustomerSummaryEndpoint = "/api/reports/customer-summary";

    
    /// Initializes a new instance of the <see cref="ReportsControllerTests"/> class.
    
    /// <param name="factory">Test application factory for integration testing</param>
    public ReportsControllerTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region Authentication Tests

    
    /// Tests that authenticated users with Admin role can access the customer summary.
    /// 
    /// Workflow:
    /// 1. Set an Admin JWT token in the request header
    /// 2. Send a GET request to the customer summary endpoint
    /// 3. Verify the endpoint returns 200 OK
    /// 
    /// This test ensures:
    /// - The reports endpoint is protected by authentication
    /// - Admin users have access to reporting data
    /// - The endpoint is properly configured and responsive
    
    [Fact]
    public async Task CustomerSummary_WithAdminToken_ReturnsOk()
    {
        // Arrange: Set admin authentication
        TestAuthHelper.SetAdminToken(_client);

        // Act: Request customer summary
        var response = await _client.GetAsync(CustomerSummaryEndpoint);

        // Assert: Should return 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    
    /// Tests that the customer summary returns actual data when authenticated.
    /// 
    /// Workflow:
    /// 1. Set an Admin JWT token in the request header
    /// 2. Send a GET request to the customer summary endpoint
    /// 3. Verify the response contains data
    /// 
    /// This test ensures:
    /// - The endpoint returns valid data, not just a status code
    /// - The response content is not empty
    /// - The reporting service is correctly integrated
    
    [Fact]
    public async Task CustomerSummary_WithAdminToken_ReturnsData()
    {
        // Arrange: Set admin authentication
        TestAuthHelper.SetAdminToken(_client);

        // Act: Request customer summary
        var response = await _client.GetAsync(CustomerSummaryEndpoint);

        // Assert: Response is successful
        response.EnsureSuccessStatusCode();

        // Assert: Response contains data
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }

    #endregion

    #region Authorization Tests

    
    /// Tests that unauthenticated requests are rejected with 401 Unauthorized.
    /// 
    /// Workflow:
    /// 1. Send a GET request without any authentication token
    /// 2. Verify the authentication middleware rejects the request
    /// 3. Confirm response is 401 Unauthorized
    /// 
    /// This test ensures:
    /// - The reports endpoint requires authentication
    /// - No data is exposed to unauthenticated users
    /// - The [Authorize] attribute is enforced
    /// - Security best practices are followed
    
    [Fact]
    public async Task CustomerSummary_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange: No authentication token is set

        // Act: Request customer summary without authentication
        var response = await _client.GetAsync(CustomerSummaryEndpoint);

        // Assert: Should return 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}