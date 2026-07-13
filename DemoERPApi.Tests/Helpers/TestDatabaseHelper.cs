using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DemoERPApi.Tests.Fixtures;

public class TestDatabaseHelper
{
    private readonly string _connectionString;

    public TestDatabaseHelper()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        _connectionString =
            config.GetConnectionString("DemoERPConnection")
            ?? throw new Exception("Missing TestDb connection string in configuration");
    }

    // =====================================================
    // RESET CUSTOMER
    // =====================================================
    public async Task ResetCustomer(string id)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(@"
            DELETE FROM CustomerAccess WHERE CRMCustomerID = @id;
            UPDATE Customer SET IsDeleted = 1 WHERE CRMCustomerID = @id;
        ", conn);

        cmd.Parameters.AddWithValue("@id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    // =====================================================
    // HARD DELETE
    // =====================================================
    public async Task DeleteCustomer(string crmId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(@"
DELETE FROM CustomerAccess WHERE CRMCustomerID = @id;
DELETE FROM Users WHERE CustomerID = @id;
DELETE FROM Customers WHERE CRMCustomerID = @id;
        ", conn);

        cmd.Parameters.AddWithValue("@id", crmId);

        await cmd.ExecuteNonQueryAsync();
    }

    // =====================================================
    // RESET DB
    // =====================================================
    public async Task ResetDatabase()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(@"
            DELETE FROM CustomerAccess;
            DELETE FROM Users;
            DELETE FROM Customers;
        ", conn);

        await cmd.ExecuteNonQueryAsync();
    }







    // =====================================================
    // SEED DATA (FIXED FK ORDER)
    // =====================================================
    public async Task SeedCustomers()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand(@"

        -- =========================
        -- CUSTOMERS FIRST
        -- =========================
        IF NOT EXISTS (SELECT 1 FROM Customers WHERE CRMCustomerID = 'CRM001')
            INSERT INTO Customers (CRMCustomerID, FirstName, LastName, Email, Phone, IsDeleted)
            VALUES ('CRM001', 'admin', 'Test', 'admin@test.com', '6041111001', 0);
        IF NOT EXISTS (SELECT 1 FROM Customers WHERE CRMCustomerID = 'CRM100')
            INSERT INTO Customers (CRMCustomerID, FirstName, LastName, Email, Phone, IsDeleted)
            VALUES ('CRM100', 'Michael', 'Test', 'michael@test.com', '6041111111', 0);
        IF NOT EXISTS (SELECT 1 FROM Customers WHERE CRMCustomerID = 'CRM101')
            INSERT INTO Customers (CRMCustomerID, FirstName, LastName, Email, Phone, IsDeleted)
            VALUES ('CRM101', 'Sarah', 'Test', 'sarah@test.com', '6042222222', 0);
        IF NOT EXISTS (SELECT 1 FROM Customers WHERE CRMCustomerID = 'CRM103')
            INSERT INTO Customers (CRMCustomerID, FirstName, LastName, Email, Phone, IsDeleted)
            VALUES ('CRM103', 'Owner', 'User', 'owner@test.com', '6043333103', 0);
        IF NOT EXISTS (SELECT 1 FROM Customers WHERE CRMCustomerID = 'CRM104')
            INSERT INTO Customers (CRMCustomerID, FirstName, LastName, Email, Phone, IsDeleted)
            VALUES ('CRM104', 'customerA', 'User', 'customer_user@test.com', '6043333104', 0);
        IF NOT EXISTS (SELECT 1 FROM Customers WHERE CRMCustomerID = 'CRM105')
            INSERT INTO Customers (CRMCustomerID, FirstName, LastName, Email, Phone, IsDeleted)
            VALUES ('CRM105', 'qaserA', 'User', 'qa_user@test.com', '6043333105', 0);
        IF NOT EXISTS (SELECT 1 FROM Customers WHERE CRMCustomerID = 'CRM106')
            INSERT INTO Customers (CRMCustomerID, FirstName, LastName, Email, Phone, IsDeleted)
            VALUES ('CRM106', 'qauserB', 'User', 'qa_user2@test.com', '6043333106', 0);
        IF NOT EXISTS (SELECT 1 FROM Customers WHERE CRMCustomerID = 'CRM300')
            INSERT INTO Customers (CRMCustomerID, FirstName, LastName, Email, Phone, IsDeleted)
            VALUES ('CRM300', 'Other', 'User', 'other@test.com', '6044444444', 0);
        IF NOT EXISTS (SELECT 1 FROM Customers WHERE CRMCustomerID = 'CRM301')
            INSERT INTO Customers (CRMCustomerID, FirstName, LastName, Email, Phone, IsDeleted)
            VALUES ('CRM301', 'Owner2', 'User', 'owner2@test.com', '6045555555', 0);


        -- =========================
        -- USERS SECOND
        -- =========================
        IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
        BEGIN
            INSERT INTO Users (Username, PasswordHash, Role, CustomerID, IsActive)
            VALUES ('admin', '$2a$11$9rf5SGDdrN3sSG7WV0mkCOh3vccT5oSV4KkKn6AJmQBzN28RwBZpW', 'Admin', 'CRM001', 1);
        END;
        IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'customer_user')
        BEGIN
            INSERT INTO Users (Username, PasswordHash, Role, CustomerID, IsActive)
            VALUES ('customer_user', '$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu', 'Customer', 'CRM104', 1);
        END;
        IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'qaserA')
        BEGIN
            INSERT INTO Users (Username, PasswordHash, Role, CustomerID, IsActive)
            VALUES ('qaserA', '$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu', 'QA', 'CRM105', 1);
        END;
        IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'qauserB')
        BEGIN
            INSERT INTO Users (Username, PasswordHash, Role, CustomerID, IsActive)
            VALUES ('qauserB', '$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu', 'QA', 'CRM106', 1);
        END;
        IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'michael')
        BEGIN
            INSERT INTO Users (Username, PasswordHash, Role, CustomerID, IsActive)
            VALUES ('michael', '$2a$11$cEqt9HRcapp4ERxKPh7bnOTWFo72a6IhR.5UXslaZCJbfz/kk3uny', 'Customer', 'CRM100', 1);
        END;
        IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'sarah')
        BEGIN
            INSERT INTO Users (Username, PasswordHash, Role, CustomerID, IsActive)
            VALUES ('sarah', '$2a$11$CTGbjzKYsBFyA0qskjP.OboAY/0QQZKFEdgV4ArDK8d4cqn.WCiq', 'Customer', 'CRM101', 1);
        END;
        IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'owner1')
        BEGIN
            INSERT INTO Users (Username, PasswordHash, Role, CustomerID, IsActive)
            VALUES ('owner1', '$2a$11$VudmCpNX1/dNpyWpUJwJ3uI/tXyseOCGj.1yFP9vwEbla4c.17qcy', 'Customer', 'CRM103', 1);
        END;
        IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'Owner2')
        BEGIN
            INSERT INTO Users (Username, PasswordHash, Role, CustomerID, IsActive)
            VALUES ('Owner2', '$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu', 'Customer', 'CRM301', 1);
        END;

        -- =========================
        -- CUSTOMER ACCESS LAST
        -- =========================
        IF NOT EXISTS (SELECT 1 FROM CustomerAccess WHERE CRMCustomerID = 'CRM001' AND Username = 'admin')
            INSERT INTO CustomerAccess (CRMCustomerID, Username) VALUES ('CRM001', 'admin');
        IF NOT EXISTS (SELECT 1 FROM CustomerAccess WHERE CRMCustomerID = 'CRM100' AND Username = 'michael')
            INSERT INTO CustomerAccess (CRMCustomerID, Username) VALUES ('CRM100', 'michael');
        IF NOT EXISTS (SELECT 1 FROM CustomerAccess WHERE CRMCustomerID = 'CRM101' AND Username = 'sarah')
            INSERT INTO CustomerAccess (CRMCustomerID, Username) VALUES ('CRM101', 'sarah');
        IF NOT EXISTS (SELECT 1 FROM CustomerAccess WHERE CRMCustomerID = 'CRM103' AND Username = 'owner1')
            INSERT INTO CustomerAccess (CRMCustomerID, Username) VALUES ('CRM103', 'owner1');
        IF NOT EXISTS (SELECT 1 FROM CustomerAccess WHERE CRMCustomerID = 'CRM104' AND Username = 'customer_user')
            INSERT INTO CustomerAccess (CRMCustomerID, Username) VALUES ('CRM104', 'customer_user');
        IF NOT EXISTS (SELECT 1 FROM CustomerAccess WHERE CRMCustomerID = 'CRM105' AND Username = 'qaserA')
            INSERT INTO CustomerAccess (CRMCustomerID, Username) VALUES ('CRM105', 'qaserA');
        IF NOT EXISTS (SELECT 1 FROM CustomerAccess WHERE CRMCustomerID = 'CRM106' AND Username = 'qauserB')
            INSERT INTO CustomerAccess (CRMCustomerID, Username) VALUES ('CRM106', 'qauserB');
        IF NOT EXISTS (SELECT 1 FROM CustomerAccess WHERE CRMCustomerID = 'CRM301' AND Username = 'Owner2')
            INSERT INTO CustomerAccess (CRMCustomerID, Username) VALUES ('CRM301', 'Owner2');
        IF NOT EXISTS (SELECT 1 FROM CustomerAccess WHERE CRMCustomerID = 'CRM300' AND Username = 'Other')
            INSERT INTO CustomerAccess (CRMCustomerID, Username) VALUES ('CRM300', 'Other');

        -- FAILSAFE GAURD: Explicitly remove any invalid or dynamic runtime artifacts where 'admin' gets linked to non-admin customers
        DELETE FROM CustomerAccess WHERE LOWER(Username) = 'admin' AND CRMCustomerID != 'CRM001';

        ", conn);

        await cmd.ExecuteNonQueryAsync();
    }
    /*
    public async Task DeleteCustomer(string crmId)
    {
        using var conn =
            new SqlConnection(_connectionString);

        await conn.OpenAsync();

        using var cmd =
            new SqlCommand(@"
            DELETE FROM CustomerAccess
            WHERE CRMCustomerID = @id;

            DELETE FROM Users
            WHERE CustomerID = @id;

            DELETE FROM Customers
            WHERE CRMCustomerID = @id;
        ", conn);


        cmd.Parameters.AddWithValue(
            "@id",
            crmId);


        await cmd.ExecuteNonQueryAsync();
    }
    */

    public async Task AssignCustomerAccess(
    string username,
    string crmId)
    {
        using var conn =
            new SqlConnection(_connectionString);

        await conn.OpenAsync();


        using var cmd =
            new SqlCommand(@"
            INSERT INTO CustomerAccess
            (
                Username,
                CRMCustomerID
            )
            VALUES
            (
                @username,
                @crmId
            )
        ", conn);


        cmd.Parameters.AddWithValue(
            "@username",
            username);


        cmd.Parameters.AddWithValue(
            "@crmId",
            crmId);


        await cmd.ExecuteNonQueryAsync();
    }





}