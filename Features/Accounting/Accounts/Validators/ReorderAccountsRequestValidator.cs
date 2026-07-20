using Aurum.Api.Features.Accounting.Accounts.Dtos;
using FluentValidation;

namespace Aurum.Api.Features.Accounting.Accounts.Validators;

public sealed class ReorderAccountsRequestValidator : AbstractValidator<ReorderAccountsRequest>
{
    public ReorderAccountsRequestValidator()
    {
        RuleFor(x => x.OrderedIds)
            .NotEmpty()
            .WithMessage("orderedIds must contain at least one id.");
    }
}
