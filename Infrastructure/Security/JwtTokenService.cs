using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Aurum.Api.Features.Users.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Aurum.Api.Infrastructure.Security;

/// <summary>
/// Issues HS256-signed JWTs carrying the user id ("sub") and email as
/// claims. Validation of the same token happens in the JwtBearer handler
/// registered by AuthenticationServiceExtensions.AddAppAuthentication —
/// both sides read the same JwtSettings, so they always agree.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public JwtToken GenerateToken(User user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new JwtToken(accessToken, expiresAtUtc);
    }
}
