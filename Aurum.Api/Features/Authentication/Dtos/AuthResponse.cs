namespace Aurum.Api.Features.Authentication.Dtos;

public sealed class AuthenticatedUserDto
{
    public Guid Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public string? DisplayName { get; init; }
}

/// <summary>
/// Returned by register and login. Frontend note: the previous Supabase
/// flow returned a session object with access_token; this shape keeps the
/// same two top-level things (a token + the user) so the Next.js auth
/// client only needs to change field names, not its overall flow.
/// </summary>
public sealed class AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }

    public AuthenticatedUserDto User { get; init; } = null!;
}
