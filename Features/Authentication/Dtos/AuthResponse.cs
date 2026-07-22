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

/// <summary>Returned by register, login, and refresh.</summary>
public sealed class AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }

    public string RefreshToken { get; init; } = string.Empty;

    public AuthenticatedUserDto User { get; init; } = null!;
}
