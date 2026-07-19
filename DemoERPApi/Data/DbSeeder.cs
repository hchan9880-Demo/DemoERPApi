using DemoERPApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DemoERPApi.Data;

public static class DbSeeder
{



    public static void Seed(AppDbContext context)
    {
        try
        {


            // =============================
            // 1. Seed Customers FIRST
            // =============================
            SeedCustomer(context, "CRM001", "admin", "User", "admin@test.com", "6040000001");
            SeedCustomer(context, "CRM100", "Michael", "Test", "michael@test.com", "6041111100");
            SeedCustomer(context, "CRM101", "Sarah", "Test", "sarah@test.com", "6042222101");
            SeedCustomer(context, "CRM103", "Owner", "User", "owner@test.com", "6043333103");
            SeedCustomer(context, "CRM104", "customer", "one", "customer@test.com", "6043333104");
            SeedCustomer(context, "CRM105", "Other", "qaserA", "qaserA@test.com", "6044444105");
            SeedCustomer(context, "CRM106", "Other", "qauserB", "qauserB@test.com", "6044444106");
            SeedCustomer(context, "CRM300", "Other", "User", "other@test.com", "6044444300");
            SeedCustomer(context, "CRM301", "Owner2", "User", "owner2@test.com", "6045555301");

            context.SaveChanges();



            // =============================
            // 2. Seed Users SECOND (Fixed string names to match Integration Tests)
            // =============================
            SeedUser(context, "admin", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "Admin", "CRM001");

            // Fixed: changed from 'qauser' to 'qa_user'
            SeedUser(context, "qa_user", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "QA", "CRM100");

            // Fixed: changed from 'customeruser' to 'customer_user'
            SeedUser(context, "customer_user", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "Customer", "CRM104");

            SeedUser(context, "michael", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "Customer", "CRM100");
            SeedUser(context, "sarah", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "Customer", "CRM101");
            SeedUser(context, "owner1", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "Customer", "CRM103");
            SeedUser(context, "qauserB", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "QA", "CRM106");
            SeedUser(context, "qaserA", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "QA", "CRM105");
            SeedUser(context, "Owner2", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "Customer", "CRM301");
            context.SaveChanges();

           // =============================
            // 3. Seed CustomerAccess LAST (Fixed to map to updated usernames)
            // =============================
            SeedCustomerAccess(context, "CRM001", "admin");
            SeedCustomerAccess(context, "CRM100", "qa_user");       // Fixed username parameter
            SeedCustomerAccess(context, "CRM103", "owner1");
            SeedCustomerAccess(context, "CRM104", "customer_user");
            SeedCustomerAccess(context, "CRM105", "qaserA");
            SeedCustomerAccess(context, "CRM106", "qauserB");
            SeedCustomerAccess(context, "CRM100", "michael");
            SeedCustomerAccess(context, "CRM101", "sarah");
            SeedCustomerAccess(context, "CRM301", "Owner2");
       

            context.SaveChanges();

        }
        catch (DbUpdateException ex)
        {
            // Place a breakpoint here. 
            // Inspect ex.Entries to see which entity is causing the failure.
            // If it shows an entry for 'Customers', it will show you 
            // exactly which properties EF is trying to save.
            throw;
        }
    }
    private static void SeedCustomer(AppDbContext context, string crmId, string firstName, string lastName, string email, string phone)
    {
        if (!context.Customers.Any(c => c.CRMCustomerID == crmId))
        {
            context.Customers.Add(new Customers
            {
                CRMCustomerID = crmId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                IsDeleted = false
                // Ensure you are NOT setting 'Role' or 'CustomerID' (string) here!
            });
        }
    }

    private static void SeedUser(AppDbContext context, string username, string passwordHash, string role, string customerId)
    {
        if (!context.Users.Any(u => u.Username == username))
        {
            context.Users.Add(new User
            {
                Username = username,
                PasswordHash = passwordHash,
                Role = role,
                CustomerID = customerId,
                IsActive = true
            });
        }
    }
    private static void SeedCustomerAccess(AppDbContext context, string crmCustomerId, string username)
    {
        // 1. Get the user first to find their UserId
        var user = context.Users.SingleOrDefault(u => u.Username == username);
        if (user == null) return;

        // 2. Use the schema columns: Username, CRMCustomerID, and UserId
        if (!context.CustomerAccess.Any(x => x.CRMCustomerID == crmCustomerId && x.Username == username))
        {
            context.CustomerAccess.Add(new CustomerAccess
            {
                CRMCustomerID = crmCustomerId,
                Username = username,
               UserId = user.UserId // Map the integer UserId found in the Users table
            });
        }
    }
    public static void Reset(AppDbContext context)
    {
        // Clear tables using Raw SQL to bypass EF tracking/validation
        context.Database.ExecuteSqlRaw("DELETE FROM [CustomerAccess]");
        context.Database.ExecuteSqlRaw("DELETE FROM [RefreshTokens]");
        context.Database.ExecuteSqlRaw("DELETE FROM [AuditLogs]");
        context.Database.ExecuteSqlRaw("DELETE FROM [SyncLogs]");
        context.Database.ExecuteSqlRaw("DELETE FROM [Users]");
        context.Database.ExecuteSqlRaw("DELETE FROM [Customers]");

        // Do NOT call context.SaveChanges() here if you used ExecuteSqlRaw, 
        // as the deletions have already been committed to the database.

        Seed(context);
    }
}