using BCrypt.Net;
using TenantScoring.Models;

namespace TenantScoring.Core;

public sealed class TenantRegistrationService(
    ITenantApplicationStore store,
    ITenantIdGenerator tenantIdGenerator) : ITenantRegistrationService
{
    public async Task<RegistrationResult> RegisterAsync(
        RegisterTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        var countryCode = request.CountryCode.Trim();
        if (await store.ExistsAsync(countryCode, request.MobileNumber, cancellationToken))
        {
            return RegistrationResult.Duplicate;
        }

        var tenantId = await tenantIdGenerator.GenerateAsync(cancellationToken);
        var application = new TenantApplication(
            tenantId,
            countryCode,
            request.MobileNumber,
            request.FirstName.Trim(),
            request.LastName.Trim(),
            request.Email.Trim(),
            BCrypt.Net.BCrypt.HashPassword(request.Pin));

        await store.AddAsync(application, cancellationToken);
        return RegistrationResult.Created(
            new RegisterTenantResponse(application.TenantId, "Tenant Successfully Registered"));
    }
}
