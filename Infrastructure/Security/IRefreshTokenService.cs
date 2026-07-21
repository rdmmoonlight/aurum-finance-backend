using Aurum.Api.Features.Authentication.Entities;

namespace Aurum.Api.Infrastructure.Security;

/// <summary>
/// Owns the full lifecycle of refresh tokens: issue, validate, rotate,
/// revoke. Refresh tokens are opaque random strings (not JWTs) — only their
/// SHA-256 hash lives in the database, so a database leak alone can't be
/// replayed as a live session.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>Issues and persists a new refresh token, returning the raw value to hand to the client.</summary>
    Task<string> IssueAsync(Guid userId, string? createdByIp, CancellationToken ct = default);

    /// <summary>Looks up the raw token by its hash. Throws UnauthorizedAppException if it doesn't exist, is expired, or was already revoked.</summary>
    Task<RefreshToken> ValidateAsync(string rawToken, CancellationToken ct = default);

    /// <summary>Revokes the given (already-validated) token and issues a replacement, returning the new raw value.</summary>
    Task<string> RotateAsync(RefreshToken current, string? createdByIp, CancellationToken ct = default);

    /// <summary>Revokes a single token by its raw value. No-op if it doesn't exist or is already revoked.</summary>
    Task RevokeAsync(string rawToken, string? revokedByIp, CancellationToken ct = default);

    /// <summary>Revokes every active token for a user — used on password reset, since a reset should kill every existing session.</summary>
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default);
}
