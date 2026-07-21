namespace Aurum.Api.Features.Authentication.Dtos;

public sealed class AuthenticatedUserDto
{
    public Guid Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public string? FullName { get; init; }

    public string Role { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public bool EmailConfirmed { get; init; }
}

/// <summary>
/// Returned by register and login. Frontend note: the previous Supabase
/// flow returned a session object with access_token; this shape keeps the
/// same top-level things (tokens + the user) so the Next.js auth client
/// only needs to change field names, not its overall flow. RefreshToken is
/// additive to the original contract — safe for a frontend that doesn't
/// read it yet.
/// </summary>
public sealed class AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }

    public string RefreshToken { get; init; } = string.Empty;

    public AuthenticatedUserDto User { get; init; } = null!;
}
