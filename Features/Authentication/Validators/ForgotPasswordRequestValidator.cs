using Aurum.Api.Features.Authentication.Dtos;
using FluentValidation;

namespace Aurum.Api.Features.Authentication.Validators;

public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
