using Microsoft.EntityFrameworkCore;
using TenantScoring.Core;
using TenantScoring.Models;

namespace TenantScoring.Data;

public sealed class TenantApplicationStore(TenantDbContext dbContext) : ITenantApplicationStore
{
    public Task<bool> ExistsAsync(
        string countryCode,
        string mobileNumber,
        CancellationToken cancellationToken = default) =>
        dbContext.TenantApplications.AnyAsync(
            application => application.CountryCode == countryCode && application.MobileNumber == mobileNumber,
            cancellationToken);

    public async Task AddAsync(TenantApplication application, CancellationToken cancellationToken = default)
    {
        await dbContext.TenantApplications.AddAsync(application, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
