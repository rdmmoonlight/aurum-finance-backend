using Aurum.Api.Features.Users.Entities;

namespace Aurum.Api.Infrastructure.Security;

public interface IJwtTokenService
{
    /// <summary>
    /// Issues a signed access token for the given user.
    /// </summary>
    JwtToken GenerateToken(User user);
}

/// <summary>
/// The signed token plus the moment it stops being valid, so callers (the
/// Authentication feature) can surface expiry to the client without
/// re-parsing the token.
/// </summary>
public sealed record JwtToken(string AccessToken, DateTime ExpiresAtUtc);
