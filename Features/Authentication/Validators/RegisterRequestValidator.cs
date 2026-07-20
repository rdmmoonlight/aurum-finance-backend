using Aurum.Api.Features.Authentication.Dtos;
using FluentValidation;

namespace Aurum.Api.Features.Authentication.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(120)
            .When(x => x.DisplayName is not null);
    }
}
