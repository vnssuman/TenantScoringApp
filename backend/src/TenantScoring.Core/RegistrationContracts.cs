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
    Task<TenantApplication?> FindAsync(string countryCode, string mobileNumber, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
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

public interface IAuthenticationService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ResetPinResult> ResetPinAsync(ResetPinRequest request, CancellationToken cancellationToken = default);
}

public sealed record LoginResult(LoginResponse? Response, bool MobileNumberNotFound, bool InvalidPin)
{
    public static LoginResult NotFound => new(null, true, false);
    public static LoginResult Unauthorized => new(null, false, true);
    public static LoginResult Success(LoginResponse response) => new(response, false, false);
}

public sealed record ResetPinResult(ResetPinResponse? Response, bool MobileNumberNotFound, string? ValidationError)
{
    public static ResetPinResult NotFound => new(null, true, null);
    public static ResetPinResult Invalid(string message) => new(null, false, message);
    public static ResetPinResult Success => new(new ResetPinResponse("PIN successfully changed"), false, null);
}

public sealed class TenantIdGenerationException : Exception
{
    public TenantIdGenerationException()
        : base("Tenant ID generation failure")
    {
    }
}
