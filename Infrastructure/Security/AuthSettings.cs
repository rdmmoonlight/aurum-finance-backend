namespace Aurum.Api.Infrastructure.Security;

/// <summary>
/// Strongly-typed binding for the "Auth" configuration section — thresholds
/// for account lockout and the lifetime of one-time tokens (password reset,
/// email verification). Kept separate from JwtSettings since these govern
/// application-level policy, not token signing.
/// </summary>
public sealed class AuthSettings
{
    public const string SectionName = "Auth";

    /// <summary>Consecutive failed logins before the account is temporarily locked.</summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>How long a lockout lasts once triggered.</summary>
    public int LockoutMinutes { get; set; } = 15;

    public int EmailVerificationTokenExpiryHours { get; set; } = 24;

    public int PasswordResetTokenExpiryMinutes { get; set; } = 60;
}
