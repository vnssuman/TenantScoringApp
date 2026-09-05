namespace TenantScoring.Models;

public sealed record LoginRequest(string CountryCode, string MobileNumber, string Pin);

public sealed record LoginResponse(string TenantId, string FirstName, string Token);

public sealed record ResetPinRequest(
    string CountryCode,
    string MobileNumber,
    string NewPin,
    string ConfirmNewPin);

public sealed record ResetPinResponse(string Message);
