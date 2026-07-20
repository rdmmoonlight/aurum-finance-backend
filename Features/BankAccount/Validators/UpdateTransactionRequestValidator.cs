using Aurum.Api.Features.BankAccount.Dtos;
using FluentValidation;

namespace Aurum.Api.Features.BankAccount.Validators;

public sealed class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionRequestValidator()
    {
        RuleFor(x => x.Category.Value)
            .MaximumLength(60)
            .When(x => x.Category.IsSet && x.Category.Value is not null);

        RuleFor(x => x.Notes.Value)
            .MaximumLength(280)
            .When(x => x.Notes.IsSet && x.Notes.Value is not null);
    }
}
