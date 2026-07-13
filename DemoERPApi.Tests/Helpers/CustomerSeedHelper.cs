using DemoERPApi.Data;
using DemoERPApi.Models;
using DemoERPApi.Tests.Fixtures;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Xunit.Abstractions;
using static DemoERPApi.Tests.Helpers.TestData;

namespace DemoERPApi.Tests.Helpers;

public static class CustomerSeedHelper
{

        public static async Task SeedCustomer(
            HttpClient client,
            string id,
            ITestOutputHelper? output = null)
        {
            try
            {
                // =====================================================
                // 1. Login Admin
                // =====================================================

                var loginResponse =
                    await client.PostAsJsonAsync(
                        "/api/Auth/login",
                        new LoginRequest
                        {
                            Username = "admin",
                            Password = "Password123"
                        });


                if (!loginResponse.IsSuccessStatusCode)
                {
                    throw new Exception(
                        $"Admin login failed: {loginResponse.StatusCode}");
                }


                var loginBody =
                    await loginResponse.Content.ReadAsStringAsync();


                Log(output, loginBody);


                var loginResult =
                    JsonSerializer.Deserialize<LoginResultDto>(
                        loginBody,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });


                if (loginResult == null)
                    throw new Exception("Login result is null");


                var token = loginResult.GetToken();


                if (string.IsNullOrEmpty(token))
                    throw new Exception("JWT token missing");



                // =====================================================
                // 2. Set Admin JWT
                // =====================================================

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token);



                // =====================================================
                // 3. Debug JWT
                // =====================================================

                var jwt =
                    new JwtSecurityTokenHandler()
                        .ReadJwtToken(token);


                Log(output, "========== JWT CLAIMS ==========");


                foreach (var claim in jwt.Claims)
                {
                    Log(
                        output,
                        $"{claim.Type} = {claim.Value}");
                }



                // =====================================================
                // 4. Remove old test customer
                // =====================================================

                await DeleteCustomerDirectly(

                    id,
                    output);



                // =====================================================
                // 5. Seed Customer
                // =====================================================

                var customer =
                    new CustomerDto
                    {
                        CRMCustomerID = id,
                        FirstName = "Test",
                        LastName = "User",
                        Email = $"test_{id}@mail.com",
                        Phone = "6041234567"
                    };


                var response =
                    await client.PostAsJsonAsync(
                        "/api/Customer/sync",
                        customer);



                var body =
                    await response.Content.ReadAsStringAsync();



                Log(
                    output,
                    $"SYNC STATUS: {response.StatusCode}");


                Log(
                    output,
                    body);



                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    output?.WriteLine($"Customer {id} already exists. Skipping seed.");
                    return;
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(
                        $"Customer seed failed. Status={response.StatusCode}. Body={body}");
                }





            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"CustomerSeedHelper failed: {ex.Message}",
                    ex);
            }
        }




    /*
    public static async Task SeedCustomer(
    HttpClient client,
    string id,
    ITestOutputHelper? output = null)
    {
        try
        {
            // =====================================================
            // Existing seed logic remains unchanged
            // =====================================================

            await DeleteCustomerDirectly(
                id,
                output);


            var customer =
                new CustomerDto
                {
                    CRMCustomerID = id,
                    FirstName = "Test",
                    LastName = "User",
                    Email = $"test_{id}@mail.com",
                    Phone = "6041234567"
                };


            var response =
                await client.PostAsJsonAsync(
                    "/api/Customer/sync",
                    customer);


            var body =
                await response.Content.ReadAsStringAsync();


            Log(
                output,
                $"SYNC STATUS: {response.StatusCode}");

            Log(
                output,
                body);


            if (!response.IsSuccessStatusCode &&
                response.StatusCode != HttpStatusCode.Conflict)
            {
                throw new Exception(
                    $"Customer seed failed. Status={response.StatusCode}. Body={body}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"CustomerSeedHelper failed: {ex.Message}",
                ex);
        }
    }



    */


    public static async Task VerifyCustomerAccess(
     IServiceProvider services,
     string username,
     string customerId,
     ITestOutputHelper output)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();


        var access = await db.CustomerAccess
            .FirstOrDefaultAsync(x =>
                x.Username == username &&
                x.CRMCustomerID == customerId);



        output.WriteLine(
            $"Looking for CustomerAccess: Username={username}, CustomerId={customerId}");


        output.WriteLine(
            access == null
                ? "CustomerAccess NOT FOUND"
                : $"Found: {access.Username} -> {access.CRMCustomerID}");


        Assert.NotNull(access);
    }

    public static async Task AssignCustomerAccess(
    string crmCustomerId,
    string username,
    ITestOutputHelper? output = null)
    {
        try
        {
            var connectionString =
  "Server=localhost\\SQLEXPRESS;Database=DemoERP;Trusted_Connection=True;TrustServerCertificate=True";

            await using var conn =
                new SqlConnection(connectionString);


            await conn.OpenAsync();


            using var cmd =
                new SqlCommand(@"
                IF NOT EXISTS
                (
                    SELECT 1
                    FROM CustomerAccess
                    WHERE CRMCustomerID=@id
                    AND LOWER(Username)=LOWER(@username)
                )
                BEGIN
                    INSERT INTO CustomerAccess
                    (
                        CRMCustomerID,
                        Username
                    )
                    VALUES
                    (
                        @id,
                        @username
                    )
                END
            ", conn);



            cmd.Parameters.AddWithValue(
                "@id",
                crmCustomerId);


            cmd.Parameters.AddWithValue(
                "@username",
                username);



            await cmd.ExecuteNonQueryAsync();



            output?.WriteLine(
                $"CustomerAccess created: {crmCustomerId} -> {username}");
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"AssignCustomerAccess failed: {ex.Message}",
                ex);
        }
    }

    private static async Task DeleteCustomerDirectly(
    string id,
    ITestOutputHelper? output)
    {
        var connectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=DemoERP_Test;Trusted_Connection=True;TrustServerCertificate=True;";

        await using var conn =
            new SqlConnection(connectionString);

        await conn.OpenAsync();

        //
        // Delete CustomerAccess
        //
        var accessCmd = new SqlCommand(@"
DELETE FROM CustomerAccess
WHERE CRMCustomerID=@id
", conn);

        accessCmd.Parameters.AddWithValue("@id", id);

        await accessCmd.ExecuteNonQueryAsync();

        //
        // Delete Users
        //
        var userCmd = new SqlCommand(@"
DELETE FROM Users
WHERE CustomerID=@id
", conn);

        userCmd.Parameters.AddWithValue("@id", id);

        await userCmd.ExecuteNonQueryAsync();

        //
        // Delete Customer
        //
        var customerCmd = new SqlCommand(@"
DELETE FROM Customers
WHERE CRMCustomerID=@id
", conn);

        customerCmd.Parameters.AddWithValue("@id", id);

        await customerCmd.ExecuteNonQueryAsync();

        output?.WriteLine($"Deleted customer {id}");
    }




    private static void Log(
        ITestOutputHelper? output,
        string message)
    {
        if (output != null)
            output.WriteLine(message);
        else
            Console.WriteLine(message);
    }
}





public class LoginResultDto
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }


    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }



    public string GetToken()
    {
        return Token ??
               AccessToken ??
               string.Empty;
    }
}