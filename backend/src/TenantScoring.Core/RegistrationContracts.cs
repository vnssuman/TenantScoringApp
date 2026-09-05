using TenantScoring.Models;

namespace TenantScoring.Core;

public interface ITenantIdGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}

public interface ITenantApplicationStore
{
    Task<bool> ExistsAsync(string countryCode, string mobileNumber, CancellationToken cancellationToken = default);
    Task AddAsync(TenantApplication application, CancellationToken cancellationToken = default);
}

public sealed record RegistrationResult(bool IsDuplicate, RegisterTenantResponse? Response)
{
    public static RegistrationResult Duplicate => new(true, null);

    public static RegistrationResult Created(RegisterTenantResponse response) => new(false, response);
}

public interface ITenantRegistrationService
{
    Task<RegistrationResult> RegisterAsync(
        RegisterTenantRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class TenantIdGenerationException : Exception
{
    public TenantIdGenerationException()
        : base("Tenant ID generation failure")
    {
    }
}
