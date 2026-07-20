using DemoERPApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DemoERPApi.Data;


/// Database seeder for test and development data.

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        try
        {
            // 1. Seed Customers
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

            // 2. Seed Users (with BCrypt hashed passwords)
            SeedUser(context, "admin", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "Admin", "CRM001");
            SeedUser(context, "qa_user", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "QA", "CRM100");
            SeedUser(context, "customer_user", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "Customer", "CRM104");
            SeedUser(context, "michael", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "Customer", "CRM100");
            SeedUser(context, "sarah", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "Customer", "CRM101");
            SeedUser(context, "owner1", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "Customer", "CRM103");
            SeedUser(context, "qauserB", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "QA", "CRM106");
            SeedUser(context, "qaserA", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "QA", "CRM105");
            SeedUser(context, "Owner2", "$2a$11$t4/Xczpvci3kHgOY8UJZUemYIxIwGXjlSWSJ7wHTf2fQLzqtDp8qu", "Customer", "CRM301");
            context.SaveChanges();

            // 3. Seed CustomerAccess (user-customer relationships)
            SeedCustomerAccess(context, "CRM001", "admin");
            SeedCustomerAccess(context, "CRM100", "qa_user");
            SeedCustomerAccess(context, "CRM103", "owner1");
            SeedCustomerAccess(context, "CRM104", "customer_user");
            SeedCustomerAccess(context, "CRM105", "qaserA");
            SeedCustomerAccess(context, "CRM106", "qauserB");
            SeedCustomerAccess(context, "CRM100", "michael");
            SeedCustomerAccess(context, "CRM101", "sarah");
            SeedCustomerAccess(context, "CRM301", "Owner2");

            context.SaveChanges();
        }
        catch (DbUpdateException)
        {
            // Inspect ex.Entries to see which entity failed
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
        var user = context.Users.SingleOrDefault(u => u.Username == username);
        if (user == null) return;

        if (!context.CustomerAccess.Any(x => x.CRMCustomerID == crmCustomerId && x.Username == username))
        {
            context.CustomerAccess.Add(new CustomerAccess
            {
                CRMCustomerID = crmCustomerId,
                Username = username,
                UserId = user.UserId
            });
        }
    }

    public static void Reset(AppDbContext context)
    {
        // Clear tables using raw SQL to bypass EF tracking
        context.Database.ExecuteSqlRaw("DELETE FROM [CustomerAccess]");
        context.Database.ExecuteSqlRaw("DELETE FROM [RefreshTokens]");
        context.Database.ExecuteSqlRaw("DELETE FROM [AuditLogs]");
        context.Database.ExecuteSqlRaw("DELETE FROM [SyncLogs]");
        context.Database.ExecuteSqlRaw("DELETE FROM [Users]");
        context.Database.ExecuteSqlRaw("DELETE FROM [Customers]");

        Seed(context);
    }
}