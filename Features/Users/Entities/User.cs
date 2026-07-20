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

    public string? DisplayName { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
