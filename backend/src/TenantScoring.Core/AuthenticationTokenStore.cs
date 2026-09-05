using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace TenantScoring.Core;

public sealed class AuthenticationTokenStore : IAuthenticationTokenStore
{
    private readonly ConcurrentDictionary<string, string> tokens = new(StringComparer.Ordinal);

    public string CreateToken(string tenantId)
    {
        Span<byte> tokenBytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(tokenBytes);
        var token = Convert.ToBase64String(tokenBytes);
        tokens[token] = tenantId;
        return token;
    }

    public bool TryGetTenantId(string token, out string tenantId) =>
        tokens.TryGetValue(token, out tenantId!);

    public void Remove(string token) => tokens.TryRemove(token, out _);
}
