namespace Aurum.Api.Features.Authentication.Entities;

/// <summary>
/// A single refresh token in a rotation chain. The raw token is only ever
/// handed to the client once, at issue time — this table stores nothing but
/// its SHA-256 hash, so a leaked database dump alone can't be replayed as a
/// live session. See IRefreshTokenService.
/// </summary>
public sealed class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    /// <summary>SHA-256 hex hash of the raw token. Never the raw token itself.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string? CreatedByIp { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public string? RevokedByIp { get; set; }

    /// <summary>Set when this token was rotated out in favor of a newer one — lets a reuse of a revoked token be detected as a possible theft.</summary>
    public Guid? ReplacedByTokenId { get; set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
}
