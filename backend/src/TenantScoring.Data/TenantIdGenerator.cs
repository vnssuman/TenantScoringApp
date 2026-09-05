using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TenantScoring.Core;

namespace TenantScoring.Data;

public sealed class TenantIdGenerator(TenantDbContext dbContext) : ITenantIdGenerator
{
    private const int MaxAttempts = 5;

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var tenantId = $"K-{GenerateDigits()}";
            if (!await dbContext.TenantApplications.AnyAsync(
                    application => application.TenantId == tenantId,
                    cancellationToken))
            {
                return tenantId;
            }
        }

        throw new TenantIdGenerationException();
    }

    private static string GenerateDigits()
    {
        Span<char> digits = stackalloc char[10];
        for (var index = 0; index < digits.Length; index++)
        {
            digits[index] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
        }

        return new string(digits);
    }
}
