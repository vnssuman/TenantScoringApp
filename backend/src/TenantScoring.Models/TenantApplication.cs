namespace TenantScoring.Models;

public sealed class TenantApplication
{
    private TenantApplication()
    {
    }

    public TenantApplication(
        string tenantId,
        string countryCode,
        string mobileNumber,
        string firstName,
        string lastName,
        string email,
        string pinHash)
    {
        TenantId = tenantId;
        CountryCode = countryCode;
        MobileNumber = mobileNumber;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PinHash = pinHash;
    }

    public string TenantId { get; private set; } = null!;
    public string CountryCode { get; private set; } = null!;
    public string MobileNumber { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PinHash { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
}
