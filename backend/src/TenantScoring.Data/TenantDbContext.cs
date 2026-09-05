using Microsoft.EntityFrameworkCore;
using TenantScoring.Models;

namespace TenantScoring.Data;

public sealed class TenantDbContext(DbContextOptions<TenantDbContext> options) : DbContext(options)
{
    public DbSet<TenantApplication> TenantApplications => Set<TenantApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TenantApplication>();
        entity.ToTable("TenantApplications");
        entity.HasKey(application => application.TenantId);
        entity.HasIndex(application => application.TenantId).IsUnique();
        entity.Property(application => application.TenantId).HasMaxLength(12).IsRequired();
        entity.Property(application => application.CountryCode).HasMaxLength(10).IsRequired();
        entity.Property(application => application.MobileNumber).HasMaxLength(10).IsRequired();
        entity.Property(application => application.FirstName).HasMaxLength(50).IsRequired();
        entity.Property(application => application.LastName).HasMaxLength(50).IsRequired();
        entity.Property(application => application.Email).HasMaxLength(100).IsRequired();
        entity.Property(application => application.PinHash).IsRequired();
        entity.Property(application => application.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();
        entity.HasIndex(application => new { application.CountryCode, application.MobileNumber })
            .IsUnique();
    }
}
