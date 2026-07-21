namespace Aurum.Api.Features.Authentication.Dtos;

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}
