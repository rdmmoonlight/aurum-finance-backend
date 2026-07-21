namespace Aurum.Api.Infrastructure.Security;

/// <summary>
/// Strongly-typed binding for the "Jwt" configuration section, plus the
/// JWT_SIGNING_KEY environment variable override (see
/// AuthenticationServiceExtensions.AddAppAuthentication). Consumed by both
/// JwtTokenService (issuing) and the JwtBearer handler (validation), so the
/// two are guaranteed to agree on issuer/audience/key.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Aurum.Api";

    public string Audience { get; set; } = "Aurum.Client";

    /// <summary>
    /// HMAC-SHA256 signing key. Must be at least 32 bytes (256 bits).
    /// Always set from the JWT_SIGNING_KEY environment variable in any
    /// deployed environment — see .env.example.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenExpiryMinutes { get; set; } = 60;

    /// <summary>Lifetime of an issued refresh token. Consumed by IRefreshTokenService, not by JwtTokenService itself (refresh tokens are opaque, not JWTs).</summary>
    public int RefreshTokenExpiryDays { get; set; } = 30;
}
