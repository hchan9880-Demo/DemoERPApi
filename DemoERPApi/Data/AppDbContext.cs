using DemoERPApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DemoERPApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customers> Customers { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<CustomerAccess> CustomerAccess { get; set; } = null!;
    public DbSet<SyncLogs> SyncLogs { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- USERS TABLE ---

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.UserId);
          //  entity.Property(u => u.Role).HasColumnName("Role"); // Explicit mapping
          //  entity.Property(u => u.CustomerID).HasColumnName("CustomerID"); // Explicit mapping[cite: 10]
        });

        modelBuilder.Entity<Customers>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(c => c.CustomerID);
            // FORCE IGNORE: This stops EF Core from looking for 'Role' or 'CustomerID' string here[cite: 10]
            entity.Ignore("Role");
        });





        // --- CUSTOMERACCESS TABLE ---
        modelBuilder.Entity<CustomerAccess>(entity =>
        {
            entity.ToTable("CustomerAccess");
            entity.HasKey(ca => ca.Id);
            entity.Property(ca => ca.CRMCustomerID).HasColumnName("CRMCustomerID");
            entity.Property(ca => ca.UserId).HasColumnName("UserId");
          //    entity.Property(u => u.Role).HasColumnName("Role"); // Explicit mapping
         //     entity.Property(u => u.CustomerID).HasColumnName("CustomerID"); // Explicit mapping[cite: 10]


        });

        // =====================================================
        // SyncLogs
        // =====================================================
        modelBuilder.Entity<SyncLogs>(entity =>
        {
            entity.ToTable("SyncLogs");
            entity.HasKey(x => x.LogId);
            entity.Property(x => x.LogId).ValueGeneratedOnAdd();
            entity.Property(x => x.Operation).HasMaxLength(40);
            entity.Property(x => x.Status).HasMaxLength(40);
            entity.Property(x => x.Message).HasMaxLength(1000);
            entity.Property(x => x.Username).HasMaxLength(200);
            entity.Property(x => x.RequestId).HasMaxLength(200);
            entity.Property(x => x.CreatedDate).HasColumnType("datetime2");
        });

        // =====================================================
        // AuditLogs
        // =====================================================
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.AuditId);
            entity.Property(e => e.EntityName).HasMaxLength(100);
            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.ChangedBy).HasMaxLength(100);
        });

        // =====================================================
        // RefreshTokens
        // =====================================================
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(e => e.TokenID);
            entity.Property(e => e.TokenID).ValueGeneratedOnAdd();
            entity.Property(e => e.Token).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.CreatedByIP).HasMaxLength(45);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime2");
            entity.Property(e => e.ExpirationDate).HasColumnType("datetime2");
            entity.Property(e => e.RevokedDate).HasColumnType("datetime2");
            entity.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .HasConstraintName("FK_RefreshTokens_Users")
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}