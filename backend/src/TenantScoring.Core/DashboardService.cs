using TenantScoring.Models;

namespace TenantScoring.Core;

public sealed class DashboardService(ITenantApplicationStore store) : IDashboardService
{
    public async Task<DashboardUserSummaryDto?> GetSummaryAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var application = await store.FindByTenantIdAsync(tenantId, cancellationToken);
        return application is null
            ? null
            : new DashboardUserSummaryDto(
                application.TenantId,
                $"{application.FirstName} {application.LastName}",
                ["Renter & Owner"],
                application.MobileNumber,
                application.Email);
    }
}
