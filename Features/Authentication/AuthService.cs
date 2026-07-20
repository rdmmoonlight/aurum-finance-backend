using Aurum.Api.Core.Exceptions;
using Aurum.Api.Features.Authentication.Dtos;
using Aurum.Api.Features.Users.Entities;
using Aurum.Api.Infrastructure.Database;
using Aurum.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api.Features.Authentication;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    Task<AuthenticatedUserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
}

/// <summary>
/// Owns credential storage and token issuing directly — the successor to
/// NestJS's SupabaseAuthMiddleware, which only verified tokens Supabase had
/// already issued. Register/Login/JWT generation are new responsibilities
/// this API now has that it didn't before.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(AppDbContext db, IPasswordHasher<User> passwordHasher, IJwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
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
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? null : request.DisplayName.Trim(),
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var normalizedEmail = Normalize(request.Email);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        // Same generic message whether the account doesn't exist or the
        // password is wrong — never reveal which one it was.
        if (user is null)
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            user.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return BuildAuthResponse(user);
    }

    public async Task<AuthenticatedUserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync(new object?[] { userId }, ct)
            ?? throw new NotFoundException("User", userId);

        return ToDto(user);
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var token = _jwtTokenService.GenerateToken(user);
        return new AuthResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc,
            User = ToDto(user),
        };
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();

    private static AuthenticatedUserDto ToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        DisplayName = user.DisplayName,
    };
}
