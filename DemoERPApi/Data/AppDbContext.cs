using DemoERPApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DemoERPApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<CustomerAccess> CustomerAccess { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ======================================
        // Customers
        // ======================================

        modelBuilder.Entity<Customer>()
            .HasKey(c => c.CRMCustomerID);

        // ======================================
        // Users
        // ======================================

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne<Customer>()
            .WithMany()
            .HasForeignKey(u => u.CustomerID)
            .HasPrincipalKey(c => c.CRMCustomerID)
            .OnDelete(DeleteBehavior.Restrict);

        // ======================================
        // CustomerAccess
        // ======================================

        modelBuilder.Entity<CustomerAccess>()
            .HasKey(ca => ca.Id);
    }
}


