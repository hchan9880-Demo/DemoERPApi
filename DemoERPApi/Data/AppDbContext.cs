using DemoERPApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Win32;

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

    public DbSet<SyncLogs> SyncLogs { get; set; }

    // Register AuditLogs DbSet
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);


        // =====================================================
        // Ignore EF Core pending model changes warning
        //
        // Reason:
        // Existing DemoERP database already exists.
        // Integration tests call Database.Migrate()
        // during startup.
        //
        // Without this, WebApplicationFactory fails before
        // executing API tests.
        // =====================================================

        optionsBuilder.ConfigureWarnings(
            warnings =>
            warnings.Ignore(
                RelationalEventId.PendingModelChangesWarning));
    }



    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {


        base.OnModelCreating(modelBuilder);



        // =====================================================
        // Customer
        // =====================================================

        modelBuilder.Entity<Customer>()
            .HasKey(c => c.CRMCustomerID);



        // =====================================================
        // User
        // =====================================================

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();


        modelBuilder.Entity<User>()
            .HasOne<Customer>()
            .WithMany()
            .HasForeignKey(u => u.CustomerID)
            .HasPrincipalKey(c => c.CRMCustomerID)
            .OnDelete(DeleteBehavior.Restrict);



        // =====================================================
        // CustomerAccess
        // =====================================================

        modelBuilder.Entity<CustomerAccess>()
            .HasKey(ca => ca.Id);



        // =====================================================
        // SyncLog
        // =====================================================
        modelBuilder.Entity<SyncLogs>()
.ToTable("SyncLogs");
        modelBuilder.Entity<SyncLogs>()
            .HasKey(x => x.LogId);


        modelBuilder.Entity<SyncLogs>()
            .Property(x => x.LogId)
            .ValueGeneratedOnAdd();


        modelBuilder.Entity<SyncLogs>()
            .Property(x => x.CRMCustomerID)
            .HasColumnName("CRMCustomerID")
            .HasMaxLength(100);


        modelBuilder.Entity<SyncLogs>()
            .Property(x => x.Operation)
            .HasColumnName("Operation")
            .HasMaxLength(40);


        modelBuilder.Entity<SyncLogs>()
            .Property(x => x.Status)
            .HasColumnName("Status")
            .HasMaxLength(40);


        modelBuilder.Entity<SyncLogs>()
            .Property(x => x.Message)
            .HasColumnName("Message")
            .HasMaxLength(1000);


        modelBuilder.Entity<SyncLogs>()
            .Property(x => x.Username)
            .HasColumnName("Username")
            .HasMaxLength(200);


        modelBuilder.Entity<SyncLogs>()
            .Property(x => x.RequestId)
            .HasColumnName("RequestId")
            .HasMaxLength(200);


        modelBuilder.Entity<SyncLogs>()
            .Property(x => x.CreatedDate)
            .HasColumnName("CreatedDate")
            .HasColumnType("datetime2");


        modelBuilder.Entity<SyncLogs>()
            .Property(x => x.ExecutionTimeMs)
            .HasColumnName("ExecutionTimeMs");


        // =====================================================
        // Optional:
        // Match database column names
        // =====================================================

        modelBuilder.Entity<SyncLogs>()
            .Property(x => x.CreatedDate)
            .HasColumnType("datetime2");





        // =====================================================
        // AuditLogs
        // =====================================================



        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId);

            entity.ToTable("AuditLogs");

            entity.Property(e => e.EntityName)
                .HasMaxLength(100);

            entity.Property(e => e.Action)
                .HasMaxLength(50);

            entity.Property(e => e.ChangedBy)
                .HasMaxLength(100);
        });









    }
}