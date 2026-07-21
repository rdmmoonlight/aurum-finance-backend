namespace Aurum.Api.Features.Authentication.Dtos;

public sealed class ResendVerificationRequest
{
    public string Email { get; init; } = string.Empty;
}
