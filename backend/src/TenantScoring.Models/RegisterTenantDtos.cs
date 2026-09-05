namespace TenantScoring.Models;

public sealed record RegisterTenantRequest(
    string CountryCode,
    string FirstName,
    string LastName,
    string MobileNumber,
    string Email,
    string Pin,
    string ConfirmPin);

public sealed record RegisterTenantResponse(string TenantId, string Message);
