using Microsoft.EntityFrameworkCore;
using TenantScoring.Core;
using TenantScoring.Models;

namespace TenantScoring.Data;

public sealed class KirayeeRepository(TenantDbContext dbContext) : IKirayeeRepository
{
    public Task<User?> FindUserAsync(Guid userId, CancellationToken cancellationToken) => dbContext.Users.AsTracking().Include(user => user.Profile).SingleOrDefaultAsync(user => user.UserId == userId, cancellationToken);
    public Task<User?> FindUserByMobileAsync(string mobileNumber, CancellationToken cancellationToken) => dbContext.Users.AsTracking().SingleOrDefaultAsync(user => user.MobileNumber == mobileNumber, cancellationToken);
    public Task<User?> FindUserByEmailAsync(string email, CancellationToken cancellationToken) => dbContext.Users.AsTracking().SingleOrDefaultAsync(user => user.Email == email, cancellationToken);

    public async Task AddUserAsync(User user, UserProfile profile, CancellationToken cancellationToken)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.UserProfiles.AddAsync(profile, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<UserProfile?> FindProfileAsync(Guid userId, CancellationToken cancellationToken) => dbContext.UserProfiles.AsTracking().SingleOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
    public Task<List<BehavioralQuestion>> GetBehavioralQuestionsAsync(CancellationToken cancellationToken) => dbContext.BehavioralQuestions.AsNoTracking().Where(question => question.IsActive).OrderBy(question => question.Category).ToListAsync(cancellationToken);
    public Task<List<TenantBehavioralResponse>> GetBehavioralResponsesAsync(Guid userId, CancellationToken cancellationToken) => dbContext.TenantBehavioralResponses.Where(response => response.UserId == userId).ToListAsync(cancellationToken);
    public async Task ReplaceBehavioralResponsesAsync(Guid userId, IEnumerable<TenantBehavioralResponse> responses, CancellationToken cancellationToken)
    {
        var existingResponses = await dbContext.TenantBehavioralResponses
            .Where(response => response.UserId == userId)
            .ToListAsync(cancellationToken);
        var replacements = responses.ToDictionary(response => response.QuestionId);

        foreach (var existing in existingResponses)
        {
            if (replacements.Remove(existing.QuestionId, out var replacement))
            {
                existing.UpdateAnswer(replacement.UserAnswer, replacement.PointsEarned);
            }
            else
            {
                dbContext.TenantBehavioralResponses.Remove(existing);
            }
        }

        await dbContext.TenantBehavioralResponses.AddRangeAsync(replacements.Values, cancellationToken);
    }
    public async Task AddBankAccountAsync(OwnerBankAccount account, CancellationToken cancellationToken) { await dbContext.OwnerBankAccounts.AddAsync(account, cancellationToken); await dbContext.SaveChangesAsync(cancellationToken); }
    public async Task<IReadOnlyList<OwnerBankAccount>> GetBankAccountsAsync(Guid ownerId, CancellationToken cancellationToken) => await dbContext.OwnerBankAccounts.AsNoTracking().Where(account => account.OwnerId == ownerId && account.IsActive).ToListAsync(cancellationToken);
    public async Task AddPropertyAsync(Property property, CancellationToken cancellationToken) { await dbContext.Properties.AddAsync(property, cancellationToken); await dbContext.SaveChangesAsync(cancellationToken); }
    public async Task<IReadOnlyList<Property>> GetPropertiesAsync(Guid? ownerId, CancellationToken cancellationToken) => await dbContext.Properties.AsNoTracking().Include(property => property.Leases).Where(property => ownerId == null || property.OwnerId == ownerId).ToListAsync(cancellationToken);
    public Task<Property?> FindPropertyAsync(Guid propertyId, CancellationToken cancellationToken) => dbContext.Properties.Include(property => property.Leases).SingleOrDefaultAsync(property => property.PropertyId == propertyId, cancellationToken);
    public async Task<IReadOnlyList<User>> SearchTenantsAsync(string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || !query.All(character => character is >= '0' and <= '9')) return [];
        var users = dbContext.Users.AsNoTracking().Include(user => user.Profile)
            .Where(user => (user.CurrentRole == "Tenant" || user.CurrentRole == "Both") && user.Profile != null);
        return await users.Where(user => user.MobileNumber.StartsWith(query))
            .OrderBy(user => user.MobileNumber).Take(10).ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<Lease>> GetOwnerLeasesAsync(Guid ownerId, string? status, CancellationToken cancellationToken)
    {
        var query = dbContext.Leases.AsNoTracking().Include(lease => lease.Property).Include(lease => lease.Tenant).ThenInclude(tenant => tenant.Profile)
            .Where(lease => lease.OwnerId == ownerId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(lease => lease.Status == status);
        return await query.OrderByDescending(lease => lease.CreatedAt).ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<Lease>> GetTenantRentalRequestsAsync(Guid tenantId, string? status, CancellationToken cancellationToken)
    {
        var query = dbContext.Leases.AsNoTracking()
            .Include(lease => lease.Property)
            .Include(lease => lease.Owner).ThenInclude(owner => owner.Profile)
            .Where(lease => lease.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(lease => lease.Status == status);
        return await query.OrderByDescending(lease => lease.CreatedAt).ToListAsync(cancellationToken);
    }
    public async Task AddScoreAsync(TenantScore score, CancellationToken cancellationToken) { await dbContext.TenantScores.AddAsync(score, cancellationToken); await dbContext.SaveChangesAsync(cancellationToken); }
    public Task<TenantScore?> GetLatestScoreAsync(Guid tenantId, CancellationToken cancellationToken) => dbContext.TenantScores.Where(score => score.TenantId == tenantId).OrderByDescending(score => score.CalculatedAt).FirstOrDefaultAsync(cancellationToken);
    public async Task AddLeaseAsync(Lease lease, CancellationToken cancellationToken) { await dbContext.Leases.AddAsync(lease, cancellationToken); await dbContext.SaveChangesAsync(cancellationToken); }
    public Task<Lease?> FindLeaseAsync(Guid leaseId, CancellationToken cancellationToken) => dbContext.Leases.Include(lease => lease.Property).Include(lease => lease.Tenant).ThenInclude(tenant => tenant.Profile).SingleOrDefaultAsync(lease => lease.LeaseId == leaseId, cancellationToken);
    public async Task<IReadOnlyList<Lease>> GetLeasesAsync(Guid userId, CancellationToken cancellationToken) => await dbContext.Leases.AsNoTracking().Where(lease => lease.TenantId == userId || lease.OwnerId == userId).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<Lease>> GetTenantPaymentLeasesAsync(Guid tenantId, CancellationToken cancellationToken) => await dbContext.Leases.AsNoTracking()
        .Include(lease => lease.Property)
        .Include(lease => lease.Payments)
        .Where(lease => lease.TenantId == tenantId && (lease.Status == "Active" || lease.Status == "CancellationRequested" || lease.Status == "Terminated"))
        .OrderByDescending(lease => lease.StartDate)
        .ToListAsync(cancellationToken);
    public Task AddPaymentsAsync(IEnumerable<RentPayment> payments, CancellationToken cancellationToken) => dbContext.RentPayments.AddRangeAsync(payments, cancellationToken);
    public async Task AddPaymentAsync(RentPayment payment, CancellationToken cancellationToken) { await dbContext.RentPayments.AddAsync(payment, cancellationToken); await dbContext.SaveChangesAsync(cancellationToken); }
    public Task<RentPayment?> FindPaymentAsync(Guid paymentId, CancellationToken cancellationToken) => dbContext.RentPayments.Include(payment => payment.Lease).SingleOrDefaultAsync(payment => payment.PaymentId == paymentId, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}