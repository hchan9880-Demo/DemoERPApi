using DemoERPApi.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DemoERPApi.Tests.Helpers;

public static class CustomerSeedHelper
{
    // Use API to seed (BEST for integration tests)
    public static async Task SeedCustomer(HttpClient client, string id)
    {
        // 1. CLEANUP PREVIOUS TEST DATA (Replaces the raw SQL DELETE)
        // We call the delete endpoint to ensure a blank slate for this specific test ID.
        // We don't assert success here because the customer might not exist yet.
        await client.DeleteAsync($"/api/Customer/{id}");

        var customer = new CustomerDto
        {
            CRMCustomerID = id,
            FirstName = "Test",
            LastName = "User",
            Email = $"test_{id}@mail.com",
            Phone = "6041234567"
        };

        // 2. SEED FRESH DATA
        var response = await client.PostAsJsonAsync("/api/Customer/sync", customer);

        // Optional safety: ignore duplicate conflicts in tests
        if (!response.IsSuccessStatusCode &&
            response.StatusCode != System.Net.HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new System.Exception($"Seed failed: {error}");
        }
    }
}