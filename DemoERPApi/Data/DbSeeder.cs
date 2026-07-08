using DemoERPApi.Models;

namespace DemoERPApi.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        // =============================
        // 1. Seed Customers FIRST
        // =============================

        SeedCustomer(
            context,
            "CRM001",
            "admin",
            "User",
            "admin@test.com",
            "6040000000");


        SeedCustomer(
            context,
            "CRM100",
            "Michael",
            "Test",
            "michael@test.com",
            "6041111111");


        SeedCustomer(
            context,
            "CRM101",
            "Sarah",
            "Test",
            "sarah@test.com",
            "6042222222");


        SeedCustomer(
            context,
            "CRM103",
            "Owner",
            "User",
            "owner@test.com",
            "6043333333");


        SeedCustomer(
            context,
            "CRM300",
            "Other",
            "User",
            "other@test.com",
            "6044444444");


        SeedCustomer(
            context,
            "CRM301",
            "Owner2",
            "User",
            "owner2@test.com",
            "6045555555");


        context.SaveChanges();



        // =============================
        // 2. Seed Users SECOND
        // FK:
        // Users.CustomerID
        //        ->
        // Customers.CRMCustomerID
        // =============================


        SeedUser(
            context,
            "admin",
            "$2a$11$9rf5SGDdrN3sSG7WV0mkCOh3vccT5oSV4KkKn6AJmQBzN28RwBZpW",
            "Admin",
            "CRM001");


        SeedUser(
            context,
            "qauser",
            "$2a$11$VudmCpNX1/dNpyWpUJwJ3uI/tXyseOCGj.1yFP9vwEbla4c.17qcy",
            "QA",
            "CRM100");


        SeedUser(
            context,
            "michael",
            "$2a$11$cEqt9HRcapp4ERxKPh7bnOTWFo72a6IhR.5UXslaZCJbfz/kk3uny",
            "Customer",
            "CRM100");


        SeedUser(
            context,
            "sarah",
            "$2a$11$CTGbjzKYsBFyA0qskxjP.OboAY/0QQZKFEdgV4ArDK8d4cqn.WCiq",
            "Customer",
            "CRM101");


        SeedUser(
            context,
            "owner1",
            "$2a$11$VudmCpNX1/dNpyWpUJwJ3uI/tXyseOCGj.1yFP9vwEbla4c.17qcy",
            "Customer",
            "CRM103");


        context.SaveChanges();



        // =============================
        // 3. Seed CustomerAccess LAST
        // =============================

        SeedCustomerAccess(
            context,
            "CRM100",
            "admin");


        SeedCustomerAccess(
            context,
            "CRM103",
            "owner1");


        SeedCustomerAccess(
            context,
            "CRM101",
            "sarah");


        SeedCustomerAccess(
            context,
            "CRM301",
            "owner1");


        SeedCustomerAccess(
            context,
            "CRM001",
            "admin");


        context.SaveChanges();
    }



    // =====================================================
    // CUSTOMER HELPER
    // =====================================================

    private static void SeedCustomer(
        AppDbContext context,
        string crmId,
        string firstName,
        string lastName,
        string email,
        string phone)
    {
        if (!context.Customers.Any(c => c.CRMCustomerID == crmId))
        {
            context.Customers.Add(
                new Customer
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



    // =====================================================
    // USER HELPER
    // =====================================================

    private static void SeedUser(
        AppDbContext context,
        string username,
        string passwordHash,
        string role,
        string customerId)
    {
        if (!context.Users.Any(u => u.Username == username))
        {
            context.Users.Add(
                new User
                {
                    Username = username,
                    PasswordHash = passwordHash,
                    Role = role,
                    CustomerID = customerId,
                    IsActive = true
                });
        }
    }



    // =====================================================
    // CUSTOMER ACCESS HELPER
    // =====================================================

    private static void SeedCustomerAccess(
        AppDbContext context,
        string crmCustomerId,
        string username)
    {
        if (!context.CustomerAccess.Any(
            x => x.CRMCustomerID == crmCustomerId
              && x.Username == username))
        {
            context.CustomerAccess.Add(
                new CustomerAccess
                {
                    CRMCustomerID = crmCustomerId,
                    Username = username
                });
        }
    }
    public static void Reset(AppDbContext context)
    {
        // Delete test data in FK dependency order

        context.CustomerAccess.RemoveRange(
            context.CustomerAccess);

        context.Users.RemoveRange(
            context.Users);

        context.Customers.RemoveRange(
            context.Customers);


        context.SaveChanges();


        // Restore default seed data

        Seed(context);
    }



}