namespace TenantScoring.Core;

public sealed record RentalAgreementDetails(
    Guid LeaseId, string OwnerName, string OwnerPhone, string OwnerEmail, string OwnerAddress,
    string TenantName, string TenantPhone, string TenantEmail,
    string PropertyAddress, string PropertyType, string UnitNumber,
    decimal RentAmount, decimal AdvanceAmount, DateTime StartDate, DateTime EndDate, DateTime SigningDate);

public interface IRentalAgreementDocuments
{
    Task<string> CreateAsync(RentalAgreementDetails details, CancellationToken cancellationToken);
    void Delete(string documentPath);
}
