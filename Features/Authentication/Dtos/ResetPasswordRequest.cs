namespace Aurum.Api.Features.Authentication.Dtos;

public sealed class ResetPasswordRequest
{
    public string Token { get; init; } = string.Empty;

    public string NewPassword { get; init; } = string.Empty;
}
