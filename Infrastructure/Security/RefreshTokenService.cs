using Aurum.Api.Core.Exceptions;
using Aurum.Api.Features.Authentication.Entities;
using Aurum.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Aurum.Api.Infrastructure.Security;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _db;
    private readonly JwtSettings _settings;

    public RefreshTokenService(AppDbContext db, IOptions<JwtSettings> settings)
    {
        _db = db;
        _settings = settings.Value;
    }

    public async Task<string> IssueAsync(Guid userId, string? createdByIp, CancellationToken ct = default)
    {
        var rawToken = SecureTokenGenerator.GenerateUrlSafeToken();

        var entity = new RefreshToken
        {
            UserId = userId,
            TokenHash = SecureTokenGenerator.Hash(rawToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiryDays),
            CreatedByIp = createdByIp,
        };

        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync(ct);

        return rawToken;
    }

    public async Task<RefreshToken> ValidateAsync(string rawToken, CancellationToken ct = default)
    {
        var hash = SecureTokenGenerator.Hash(rawToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (token is null || !token.IsActive)
        {
            throw new UnauthorizedAppException("Refresh token is invalid, expired, or has already been used.");
        }

        return token;
    }

    public async Task<string> RotateAsync(RefreshToken current, string? createdByIp, CancellationToken ct = default)
    {
        var rawToken = SecureTokenGenerator.GenerateUrlSafeToken();

        var replacement = new RefreshToken
        {
            UserId = current.UserId,
            TokenHash = SecureTokenGenerator.Hash(rawToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiryDays),
            CreatedByIp = createdByIp,
        };

        current.RevokedAtUtc = DateTime.UtcNow;
        current.RevokedByIp = createdByIp;
        current.ReplacedByTokenId = replacement.Id;

        _db.RefreshTokens.Add(replacement);
        await _db.SaveChangesAsync(ct);

        return rawToken;
    }

    public async Task RevokeAsync(string rawToken, string? revokedByIp, CancellationToken ct = default)
    {
        var hash = SecureTokenGenerator.Hash(rawToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (token is null || !token.IsActive)
        {
            return;
        }

        token.RevokedAtUtc = DateTime.UtcNow;
        token.RevokedByIp = revokedByIp;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var activeTokens = await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAtUtc == null && t.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        foreach (var token in activeTokens)
        {
            token.RevokedAtUtc = now;
        }

        if (activeTokens.Count > 0)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
