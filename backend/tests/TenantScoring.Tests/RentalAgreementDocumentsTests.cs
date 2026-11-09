using PdfSharp.Pdf.IO;
using TenantScoring.Core;
using TenantScoring.Data;
using Xunit;

namespace TenantScoring.Tests;

public sealed class RentalAgreementDocumentsTests
{
    [Fact]
    public async Task CreatesReadablePdfInPrivateStorage()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"rental-agreement-test-{Guid.NewGuid():N}");
        var fontPath = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf")
            : "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf";
        var documents = new RentalAgreementDocuments(directory, fontPath);
        var details = new RentalAgreementDetails(
            Guid.NewGuid(), "Owner Example", "1111111111", "owner@example.test", "Owner address",
            "Tenant Example", "2222222222", "tenant@example.test", "Property address, Pune, Maharashtra",
            "Apartment", "A-12", 20000, 30000, new DateTime(2027, 1, 1), new DateTime(2027, 12, 31), new DateTime(2026, 12, 1));
        string? filename = null;
        try
        {
            filename = await documents.CreateAsync(details, CancellationToken.None);
            Assert.Equal(filename, Path.GetFileName(filename));
            Assert.Equal(".pdf", Path.GetExtension(filename));
            var path = Path.Combine(directory, filename);
            Assert.True(File.Exists(path));
            Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString((await File.ReadAllBytesAsync(path))[..5]));
            using var pdf = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            Assert.True(pdf.PageCount > 0);
        }
        finally
        {
            if (filename is not null) documents.Delete(filename);
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

}
