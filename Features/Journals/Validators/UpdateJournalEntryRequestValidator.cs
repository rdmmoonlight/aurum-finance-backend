using Aurum.Api.Features.Journals.Dtos;
using FluentValidation;

namespace Aurum.Api.Features.Journals.Validators;

public sealed class UpdateJournalEntryRequestValidator : AbstractValidator<UpdateJournalEntryRequest>
{
    public UpdateJournalEntryRequestValidator()
    {
        RuleFor(x => x.Date)
            .InclusiveBetween(1, 31);

        RuleForEach(x => x.Rows).SetValidator(new UpdateJournalEntryRowRequestValidator());

        RuleFor(x => x.Rows)
            .Must(rows => rows.Count >= 2)
            .WithMessage("A journal entry needs at least two rows (one debit side, one credit side).");

        RuleFor(x => x.Rows)
            .Must(rows => JournalBalanceRules.IsBalanced(ToPairs(rows)))
            .WithMessage(x => JournalBalanceRules.BuildMessage(ToPairs(x.Rows)))
            .When(x => x.Rows.Count >= 2);
    }

    private static List<(decimal? Debit, decimal? Credit)> ToPairs(IEnumerable<JournalEntryRowRequest> rows) =>
        rows.Select(r => (r.Debit, r.Credit)).ToList();
}

public sealed class UpdateJournalEntryRowRequestValidator : AbstractValidator<UpdateJournalEntryRowRequest>
{
    public UpdateJournalEntryRowRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AccountRef).NotEmpty();
        RuleFor(x => x.AccountName).NotEmpty();
        RuleFor(x => x.Debit).GreaterThanOrEqualTo(0).When(x => x.Debit.HasValue);
        RuleFor(x => x.Credit).GreaterThanOrEqualTo(0).When(x => x.Credit.HasValue);
    }
}
