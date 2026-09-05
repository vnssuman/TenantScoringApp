using System.Security.Cryptography;
using BCrypt.Net;
using TenantScoring.Models;

namespace TenantScoring.Core;

public sealed class AuthenticationService(ITenantApplicationStore store) : IAuthenticationService
{
    public async Task<LoginResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var application = await store.FindAsync(
            request.CountryCode.Trim(),
            request.MobileNumber,
            cancellationToken);

        if (application is null)
        {
            return LoginResult.NotFound;
        }

        return BCrypt.Net.BCrypt.Verify(request.Pin, application.PinHash)
            ? LoginResult.Success(new LoginResponse(
                application.TenantId,
                application.FirstName,
                CreateToken()))
            : LoginResult.Unauthorized;
    }

    public async Task<ResetPinResult> ResetPinAsync(
        ResetPinRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(request.NewPin, request.ConfirmNewPin, StringComparison.Ordinal))
        {
            return ResetPinResult.Invalid("New PIN and Confirm PIN do not match.");
        }

        if (request.NewPin.Length != 4 || request.NewPin.Any(character => character is < '0' or > '9'))
        {
            return ResetPinResult.Invalid("PIN must be exactly 4 digits.");
        }

        var application = await store.FindAsync(
            request.CountryCode.Trim(),
            request.MobileNumber,
            cancellationToken);

        if (application is null)
        {
            return ResetPinResult.NotFound;
        }

        application.ChangePin(BCrypt.Net.BCrypt.HashPassword(request.NewPin));
        await store.SaveChangesAsync(cancellationToken);
        return ResetPinResult.Success;
    }

    private static string CreateToken()
    {
        Span<byte> tokenBytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(tokenBytes);
        return Convert.ToBase64String(tokenBytes);
    }
}
