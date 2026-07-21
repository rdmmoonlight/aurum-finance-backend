using System.Security.Cryptography;
using System.Text;

namespace Aurum.Api.Infrastructure.Security;

/// <summary>
/// Generates the opaque, single-use tokens used for refresh tokens,
/// password resets, and email verification. Only the SHA-256 hash of the
/// raw value is ever persisted — the raw value goes to the client (refresh
/// token) or the user's inbox (reset/verification link) and is never
/// written to the database itself. This is the same "store the hash, not
/// the secret" approach used for the password hash, applied to every
/// bearer-style secret in the system.
/// </summary>
public static class SecureTokenGenerator
{
    public static string GenerateUrlSafeToken(int byteLength = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public static string Hash(string rawToken)
    {
        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
