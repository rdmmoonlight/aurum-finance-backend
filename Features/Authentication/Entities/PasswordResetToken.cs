namespace Aurum.Api.Features.Authentication.Entities;

/// <summary>
/// A single-use token authorizing exactly one password reset. Only the
/// SHA-256 hash is persisted — the raw token is emailed to the user and
/// never stored. See AuthService.ForgotPasswordAsync / ResetPasswordAsync.
/// </summary>
public sealed class PasswordResetToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ConsumedAtUtc { get; set; }

    public bool IsActive => ConsumedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
}
