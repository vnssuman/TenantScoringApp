using Microsoft.EntityFrameworkCore;
using TenantScoring.Models;

namespace TenantScoring.Data;

public sealed class TenantDbContext(DbContextOptions<TenantDbContext> options) : DbContext(options)
{
    public DbSet<TenantApplication> TenantApplications => Set<TenantApplication>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<OwnerBankAccount> OwnerBankAccounts => Set<OwnerBankAccount>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<TenantScore> TenantScores => Set<TenantScore>();
    public DbSet<Lease> Leases => Set<Lease>();
    public DbSet<RentPayment> RentPayments => Set<RentPayment>();
    public DbSet<OwnerFeedback> OwnerFeedbacks => Set<OwnerFeedback>();
    public DbSet<BehavioralQuestion> BehavioralQuestions => Set<BehavioralQuestion>();
    public DbSet<TenantBehavioralResponse> TenantBehavioralResponses => Set<TenantBehavioralResponse>();

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

        var user = modelBuilder.Entity<User>();
        user.ToTable("Users");
        user.HasKey(item => item.UserId);
        user.HasIndex(item => item.MobileNumber).IsUnique();
        user.HasIndex(item => item.Email).IsUnique();
        user.Property(item => item.MobileNumber).HasMaxLength(15).IsRequired();
        user.Property(item => item.Email).HasMaxLength(150).IsRequired();
        user.Property(item => item.CurrentRole).HasMaxLength(20).IsRequired();
        user.Property(item => item.PinHash).IsRequired();
        user.HasOne(item => item.Profile).WithOne(item => item.User).HasForeignKey<UserProfile>(item => item.UserId).OnDelete(DeleteBehavior.Cascade);

        var profile = modelBuilder.Entity<UserProfile>();
        profile.ToTable("UserProfiles");
        profile.HasKey(item => item.ProfileId);
        profile.HasIndex(item => item.UserId).IsUnique();
        profile.Property(item => item.FirstName).HasMaxLength(100);
        profile.Property(item => item.LastName).HasMaxLength(100);
        profile.Property(item => item.ProfileImageFileName).HasMaxLength(100);
        profile.Property(item => item.PanNumber).HasMaxLength(10);
        profile.Property(item => item.AadhaarNumber).HasMaxLength(12);
        profile.Property(item => item.BehavioralScore).HasPrecision(5, 2);

        var behavioralQuestion = modelBuilder.Entity<BehavioralQuestion>();
        behavioralQuestion.ToTable("BehavioralQuestions");
        behavioralQuestion.HasKey(item => item.QuestionId);
        behavioralQuestion.Property(item => item.Category).HasMaxLength(50).IsRequired();
        behavioralQuestion.Property(item => item.QuestionText).HasMaxLength(255).IsRequired();
        behavioralQuestion.Property(item => item.PreferredAnswer).HasMaxLength(3).IsRequired();
        behavioralQuestion.HasData(
            new BehavioralQuestion(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Eviction & Disputes", "Have you ever been served an eviction notice or had a legal dispute with a landlord?", "No", 33),
            new BehavioralQuestion(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Subletting Intent", "Do you plan to sublet any part of the rented property to third parties?", "No", 33),
            new BehavioralQuestion(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Smoking Policy", "Do you smoke or use tobacco products inside residential premises?", "No", 34));

        var behavioralResponse = modelBuilder.Entity<TenantBehavioralResponse>();
        behavioralResponse.ToTable("TenantBehavioralResponses");
        behavioralResponse.HasKey(item => item.ResponseId);
        behavioralResponse.HasIndex(item => new { item.UserId, item.QuestionId }).IsUnique();
        behavioralResponse.Property(item => item.UserAnswer).HasMaxLength(3).IsRequired();
        behavioralResponse.HasOne(item => item.User).WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Cascade);
        behavioralResponse.HasOne(item => item.Question).WithMany(item => item.Responses).HasForeignKey(item => item.QuestionId).OnDelete(DeleteBehavior.Cascade);

        var bank = modelBuilder.Entity<OwnerBankAccount>();
        bank.ToTable("OwnerBankAccounts");
        bank.HasKey(item => item.BankAccountId);
        bank.Property(item => item.AccountHolderName).HasMaxLength(150).IsRequired();
        bank.Property(item => item.BankName).HasMaxLength(100).IsRequired();
        bank.Property(item => item.AccountNumber).HasMaxLength(30).IsRequired();
        bank.Property(item => item.IFSCCode).HasMaxLength(20).IsRequired();
        bank.HasOne(item => item.Owner).WithMany(item => item.BankAccounts).HasForeignKey(item => item.OwnerId).OnDelete(DeleteBehavior.Restrict);

        var property = modelBuilder.Entity<Property>();
        property.ToTable("Properties");
        property.HasKey(item => item.PropertyId);
        property.Property(item => item.PropertyName).HasMaxLength(150).IsRequired();
        property.Property(item => item.Location).HasMaxLength(512).IsRequired();
        property.Property(item => item.PropertyType).HasMaxLength(40).IsRequired();
        property.Property(item => item.Address).HasMaxLength(255).IsRequired();
        property.Property(item => item.City).HasMaxLength(100).IsRequired();
        property.Property(item => item.State).HasMaxLength(100).IsRequired();
        property.Property(item => item.RentAmount).HasPrecision(18, 2);
        property.Property(item => item.AdvanceAmount).HasPrecision(18, 2);
        property.HasOne(item => item.Owner).WithMany(item => item.Properties).HasForeignKey(item => item.OwnerId).OnDelete(DeleteBehavior.Restrict);
        property.HasOne(item => item.BankAccount).WithMany(item => item.Properties).HasForeignKey(item => item.BankAccountId).OnDelete(DeleteBehavior.SetNull);

        var score = modelBuilder.Entity<TenantScore>();
        score.ToTable("TenantScores");
        score.HasKey(item => item.ScoreId);
        score.Property(item => item.FinalScore).HasPrecision(5, 2);
        score.Property(item => item.ProfileScoreWeight).HasPrecision(5, 2);
        score.Property(item => item.CreditScoreWeight).HasPrecision(5, 2);
        score.Property(item => item.PaymentHistoryWeight).HasPrecision(5, 2);
        score.Property(item => item.OwnerFeedbackWeight).HasPrecision(5, 2);
        score.Ignore(item => item.IsExpired);
        score.HasOne(item => item.Tenant).WithMany().HasForeignKey(item => item.TenantId).OnDelete(DeleteBehavior.Restrict);

        var lease = modelBuilder.Entity<Lease>();
        lease.ToTable("Leases");
        lease.HasKey(item => item.LeaseId);
        lease.Property(item => item.AgreedRent).HasPrecision(18, 2);
        lease.Property(item => item.AgreedAdvance).HasPrecision(18, 2);
        lease.HasOne(item => item.Property).WithMany(item => item.Leases).HasForeignKey(item => item.PropertyId).OnDelete(DeleteBehavior.Restrict);
        lease.HasOne(item => item.Tenant).WithMany().HasForeignKey(item => item.TenantId).OnDelete(DeleteBehavior.Restrict);
        lease.HasOne(item => item.Owner).WithMany().HasForeignKey(item => item.OwnerId).OnDelete(DeleteBehavior.Restrict);

        var payment = modelBuilder.Entity<RentPayment>();
        payment.ToTable("RentPayments");
        payment.HasKey(item => item.PaymentId);
        payment.Property(item => item.Amount).HasPrecision(18, 2);
        payment.HasOne(item => item.Lease).WithMany(item => item.Payments).HasForeignKey(item => item.LeaseId).OnDelete(DeleteBehavior.Cascade);

        var feedback = modelBuilder.Entity<OwnerFeedback>();
        feedback.ToTable("OwnerFeedbacks");
        feedback.HasKey(item => item.FeedbackId);
        feedback.Property(item => item.ReviewText).HasMaxLength(500);
        feedback.HasOne(item => item.Lease).WithMany(item => item.Feedback).HasForeignKey(item => item.LeaseId).OnDelete(DeleteBehavior.Cascade);
        feedback.HasOne(item => item.Tenant).WithMany().HasForeignKey(item => item.TenantId).OnDelete(DeleteBehavior.Restrict);
        feedback.HasOne(item => item.Owner).WithMany().HasForeignKey(item => item.OwnerId).OnDelete(DeleteBehavior.Restrict);
    }
}
