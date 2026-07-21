namespace Aurum.Api.Features.Authentication.Dtos;

public sealed class VerifyEmailRequest
{
    public string Token { get; init; } = string.Empty;
}
