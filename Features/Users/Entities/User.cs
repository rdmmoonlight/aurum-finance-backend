namespace Aurum.Api.Features.Users.Entities;

/// <summary>
/// A registered account. Replaces Supabase's user record — with the
/// migration to a custom JWT service, this API now owns credential storage
/// directly instead of delegating identity to Supabase.
/// </summary>
public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Always stored lower-cased/trimmed — see AuthService.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>PBKDF2 hash produced by IPasswordHasher&lt;User&gt;. Never the raw password.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string? FullName { get; set; }

    /// <summary>Single-role authorization model — see UserRole.</summary>
    public UserRole Role { get; set; } = UserRole.User;

    /// <summary>Soft "account disabled" switch. Checked on every login.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Set the moment the user's email verification token is consumed. Null = unverified.</summary>
    public DateTime? EmailConfirmedAtUtc { get; set; }

    public DateTime? LastLoginAtUtc { get; set; }

    /// <summary>Consecutive failed login attempts since the last success. Reset to 0 on success.</summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>Set once FailedLoginAttempts crosses AuthSettings.MaxFailedLoginAttempts. Login is rejected until this passes.</summary>
    public DateTime? LockedUntilUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
