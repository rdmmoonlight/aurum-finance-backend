using Aurum.Api.Features.Authentication.Dtos;
using FluentValidation;

namespace Aurum.Api.Features.Authentication.Validators;

public sealed class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty();
    }
}
