using Aurum.Api.Core.Exceptions;
using Aurum.Api.Features.Authentication.Dtos;
using Aurum.Api.Features.Authentication.Entities;
using Aurum.Api.Features.Users.Entities;
using Aurum.Api.Infrastructure.Database;
using Aurum.Api.Infrastructure.Email;
using Aurum.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Aurum.Api.Features.Authentication;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ip, CancellationToken ct = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, string? ip, CancellationToken ct = default);

    Task<AuthResponse> RefreshAsync(string rawRefreshToken, string? ip, CancellationToken ct = default);

    Task LogoutAsync(string rawRefreshToken, string? ip, CancellationToken ct = default);

    Task<AuthenticatedUserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Always succeeds from the caller's point of view, whether or not the email is registered — never reveals account existence.</summary>
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);

    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);

    Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default);

    /// <summary>Always succeeds from the caller's point of view — same non-enumeration reasoning as ForgotPasswordAsync.</summary>
    Task ResendVerificationAsync(ResendVerificationRequest request, CancellationToken ct = default);
}

/// <summary>
/// Owns credential storage and token issuing directly — the successor to
/// NestJS's SupabaseAuthMiddleware, which only verified tokens Supabase had
/// already issued. Register/Login/JWT generation, refresh-token rotation,
/// password reset, email verification, and account lockout are all
/// responsibilities this API now has that it didn't before.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IEmailSender _emailSender;
    private readonly AuthSettings _authSettings;

    public AuthService(
        AppDbContext db,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IEmailSender emailSender,
        IOptions<AuthSettings> authSettings)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _emailSender = emailSender;
        _authSettings = authSettings.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ip, CancellationToken ct = default)
    {
        var normalizedEmail = Normalize(request.Email);

        var exists = await _db.Users.AnyAsync(u => u.Email == normalizedEmail, ct);
        if (exists)
        {
            throw new ConflictException($"An account with email \"{normalizedEmail}\" already exists.");
        }

        var user = new User
        {
            Email = normalizedEmail,
            FullName = string.IsNullOrWhiteSpace(request.FullName) ? null : request.FullName.Trim(),
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        await IssueEmailVerificationTokenAsync(user, ct);

        return await BuildAuthResponseAsync(user, ip, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ip, CancellationToken ct = default)
    {
        var normalizedEmail = Normalize(request.Email);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        // Same generic message whether the account doesn't exist or the
        // password is wrong — never reveal which one it was.
        if (user is null)
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAppException("This account has been deactivated.");
        }

        if (user.LockedUntilUtc is { } lockedUntil && lockedUntil > DateTime.UtcNow)
        {
            throw new UnauthorizedAppException(
                $"Too many failed login attempts. Try again after {lockedUntil:u}.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            user.FailedLoginAttempts += 1;
            if (user.FailedLoginAttempts >= _authSettings.MaxFailedLoginAttempts)
            {
                user.LockedUntilUtc = DateTime.UtcNow.AddMinutes(_authSettings.LockoutMinutes);
            }

            await _db.SaveChangesAsync(ct);
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        }

        user.FailedLoginAttempts = 0;
        user.LockedUntilUtc = null;
        user.LastLoginAtUtc = DateTime.UtcNow;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return await BuildAuthResponseAsync(user, ip, ct);
    }

    public async Task<AuthResponse> RefreshAsync(string rawRefreshToken, string? ip, CancellationToken ct = default)
    {
        var current = await _refreshTokenService.ValidateAsync(rawRefreshToken, ct);

        var user = await _db.Users.FindAsync(new object?[] { current.UserId }, ct)
            ?? throw new UnauthorizedAppException("Refresh token no longer matches an existing account.");

        if (!user.IsActive)
        {
            throw new UnauthorizedAppException("This account has been deactivated.");
        }

        var newRawRefreshToken = await _refreshTokenService.RotateAsync(current, ip, ct);
        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc,
            RefreshToken = newRawRefreshToken,
            User = ToDto(user),
        };
    }

    public Task LogoutAsync(string rawRefreshToken, string? ip, CancellationToken ct = default) =>
        _refreshTokenService.RevokeAsync(rawRefreshToken, ip, ct);

    public async Task<AuthenticatedUserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync(new object?[] { userId }, ct)
            ?? throw new NotFoundException("User", userId);

        return ToDto(user);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var normalizedEmail = Normalize(request.Email);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        // Deliberately do nothing observable if the account doesn't exist —
        // returning early here (instead of throwing NotFoundException)
        // avoids letting a caller use this endpoint to enumerate registered
        // emails.
        if (user is null || !user.IsActive)
        {
            return;
        }

        var rawToken = SecureTokenGenerator.GenerateUrlSafeToken();
        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = SecureTokenGenerator.Hash(rawToken),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_authSettings.PasswordResetTokenExpiryMinutes),
        });
        await _db.SaveChangesAsync(ct);

        await _emailSender.SendAsync(
            user.Email,
            "Reset your Aurum password",
            $"<p>Use this token to reset your password: <strong>{rawToken}</strong></p>" +
            $"<p>This token expires in {_authSettings.PasswordResetTokenExpiryMinutes} minutes. If you didn't request this, ignore this email.</p>",
            ct);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var hash = SecureTokenGenerator.Hash(request.Token);
        var token = await _db.PasswordResetTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (token is null || !token.IsActive)
        {
            throw new UnauthorizedAppException("Password reset token is invalid or has expired.");
        }

        var user = await _db.Users.FindAsync(new object?[] { token.UserId }, ct)
            ?? throw new UnauthorizedAppException("Password reset token no longer matches an existing account.");

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;
        user.FailedLoginAttempts = 0;
        user.LockedUntilUtc = null;
        token.ConsumedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        // A password reset should end every existing session, not just the
        // one that requested the reset.
        await _refreshTokenService.RevokeAllForUserAsync(user.Id, ct);
    }

    public async Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default)
    {
        var hash = SecureTokenGenerator.Hash(request.Token);
        var token = await _db.EmailVerificationTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (token is null || !token.IsActive)
        {
            throw new UnauthorizedAppException("Email verification token is invalid or has expired.");
        }

        var user = await _db.Users.FindAsync(new object?[] { token.UserId }, ct)
            ?? throw new UnauthorizedAppException("Email verification token no longer matches an existing account.");

        user.EmailConfirmedAtUtc ??= DateTime.UtcNow;
        user.UpdatedAtUtc = DateTime.UtcNow;
        token.ConsumedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    public async Task ResendVerificationAsync(ResendVerificationRequest request, CancellationToken ct = default)
    {
        var normalizedEmail = Normalize(request.Email);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        // Same non-enumeration reasoning as ForgotPasswordAsync — and
        // already-verified accounts are also a silent no-op.
        if (user is null || user.EmailConfirmedAtUtc is not null)
        {
            return;
        }

        await IssueEmailVerificationTokenAsync(user, ct);
    }

    private async Task IssueEmailVerificationTokenAsync(User user, CancellationToken ct)
    {
        var rawToken = SecureTokenGenerator.GenerateUrlSafeToken();
        _db.EmailVerificationTokens.Add(new EmailVerificationToken
        {
            UserId = user.Id,
            TokenHash = SecureTokenGenerator.Hash(rawToken),
            ExpiresAtUtc = DateTime.UtcNow.AddHours(_authSettings.EmailVerificationTokenExpiryHours),
        });
        await _db.SaveChangesAsync(ct);

        await _emailSender.SendAsync(
            user.Email,
            "Verify your Aurum email address",
            $"<p>Use this token to verify your email: <strong>{rawToken}</strong></p>" +
            $"<p>This token expires in {_authSettings.EmailVerificationTokenExpiryHours} hours.</p>",
            ct);
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(User user, string? ip, CancellationToken ct)
    {
        var token = _jwtTokenService.GenerateToken(user);
        var rawRefreshToken = await _refreshTokenService.IssueAsync(user.Id, ip, ct);

        return new AuthResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc,
            RefreshToken = rawRefreshToken,
            User = ToDto(user),
        };
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();

    private static AuthenticatedUserDto ToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FullName = user.FullName,
        Role = user.Role.ToString(),
        IsActive = user.IsActive,
        EmailConfirmed = user.EmailConfirmedAtUtc is not null,
    };
}
