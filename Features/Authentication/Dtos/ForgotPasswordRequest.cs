namespace Aurum.Api.Features.Authentication.Dtos;

public sealed class ForgotPasswordRequest
{
    public string Email { get; init; } = string.Empty;
}
