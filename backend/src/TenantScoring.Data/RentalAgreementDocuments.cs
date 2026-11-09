using System.Globalization;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using TenantScoring.Core;

namespace TenantScoring.Data;

public sealed class RentalAgreementDocuments : IRentalAgreementDocuments
{
    private readonly string directory;
    private readonly string fontPath;
    private static readonly object FontLock = new();

    public RentalAgreementDocuments(string directory, string fontPath)
    {
        this.directory = directory;
        this.fontPath = fontPath;
    }

    public async Task<string> CreateAsync(RentalAgreementDetails details, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (FontLock)
        {
            if (GlobalFontSettings.FontResolver is null)
            {
                if (!File.Exists(fontPath)) throw new FileNotFoundException("The rental agreement PDF font was not found. Configure RentalAgreements:FontPath.", fontPath);
                GlobalFontSettings.FontResolver = new AgreementFontResolver(File.ReadAllBytes(fontPath));
            }
        }
        var document = new Document();
        var normal = document.Styles["Normal"]!;
        normal.Font.Name = "AgreementFont";
        normal.Font.Size = 10;
        normal.ParagraphFormat.SpaceAfter = "6pt";
        var section = document.AddSection();
        section.PageSetup.TopMargin = "2cm";
        section.PageSetup.BottomMargin = "2cm";
        var title = section.AddParagraph("DRAFT RENTAL AGREEMENT");
        title.Format.Font.Size = 16;
        title.Format.Font.Bold = true;
        title.Format.Alignment = ParagraphAlignment.Center;
        section.AddParagraph("For review only. Acceptance in the app is not an electronic signature. The parties should obtain local legal advice on the terms, stamp duty, registration and execution requirements before relying on this document.");
        section.AddParagraph($"Request reference: {details.LeaseId}");
        section.AddParagraph($"Proposed agreement signing date: {Date(details.SigningDate)}");
        section.AddParagraph($"Owner: {details.OwnerName}\nPhone: {details.OwnerPhone}\nEmail: {details.OwnerEmail}\nPermanent address: {details.OwnerAddress}");
        section.AddParagraph($"Tenant: {details.TenantName}\nPhone: {details.TenantPhone}\nEmail: {details.TenantEmail}");
        section.AddParagraph($"Property: {details.PropertyAddress}\nProperty type: {details.PropertyType}\nUnit number: {details.UnitNumber}");
        section.AddParagraph($"Monthly rent: INR {Amount(details.RentAmount)}\nAdvance / security deposit: INR {Amount(details.AdvanceAmount)}\nLease period: {Date(details.StartDate)} to {Date(details.EndDate)}");
        section.AddParagraph("1. Premises and use. The owner proposes to let the described premises to the tenant for the stated period. The permitted use and any building or society rules should be agreed by the parties.");
        section.AddParagraph("2. Rent and deposit. The tenant will pay the monthly rent and security deposit shown above. The payment schedule, deposit deductions and return timeline must be agreed in accordance with applicable law.");
        section.AddParagraph("3. Maintenance and utilities. The parties should agree their respective responsibilities for repairs, maintenance charges, utilities and other outgoings. The tenant should promptly report damage or necessary repairs.");
        section.AddParagraph("4. Access and conduct. The tenant should use the premises responsibly and respect neighbours and applicable rules. Access by the owner should be arranged with reasonable notice except in an emergency.");
        section.AddParagraph("5. Handover and termination. At the end of the tenancy, the parties should arrange handover, assess the condition of the premises and settle outstanding amounts. Notice and termination terms should be agreed subject to applicable law.");
        section.AddParagraph("6. Applicable requirements. This draft is not a substitute for an executed agreement. Applicable state laws, stamp duty and registration requirements must be independently checked by the parties.");
        section.AddParagraph("Owner signature: _______________________    Tenant signature: _______________________");
        var renderer = new PdfDocumentRenderer { Document = document };
        renderer.RenderDocument();
        Directory.CreateDirectory(directory);
        var name = $"{Guid.NewGuid():N}.pdf";
        var path = Path.Combine(directory, name);
        try
        {
            await using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            renderer.PdfDocument.Save(stream, false);
            await stream.FlushAsync(cancellationToken);
            return name;
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
            throw;
        }
    }

    public void Delete(string documentPath) => File.Delete(Path.Combine(directory, Path.GetFileName(documentPath)));

    private static string Date(DateTime value) => value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
    private static string Amount(decimal value) => value.ToString("N2", CultureInfo.GetCultureInfo("en-IN"));

    private sealed class AgreementFontResolver(byte[] font) : IFontResolver
    {
        public byte[] GetFont(string faceName) => font;
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic) => new("AgreementFont", isBold, isItalic);
    }
}
