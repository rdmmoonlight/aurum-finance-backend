using System.Text.RegularExpressions;
using Aurum.Api.Features.BankAccount.Dtos;
using FluentValidation;

namespace Aurum.Api.Features.BankAccount.Validators;

public sealed partial class LinkAccountRequestValidator : AbstractValidator<LinkAccountRequest>
{
    [GeneratedRegex(@"^\d{10,15}$")]
    private static partial Regex AccountNumberPattern();

    public LinkAccountRequestValidator()
    {
        RuleFor(x => x.AccountHolderName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.AccountNumber)
            .Must(n => AccountNumberPattern().IsMatch(n))
            .WithMessage("accountNumber must be 10-15 digits");
    }
}
