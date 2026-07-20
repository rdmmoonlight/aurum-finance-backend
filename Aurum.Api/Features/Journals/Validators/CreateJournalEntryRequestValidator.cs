using Aurum.Api.Features.Journals.Dtos;
using FluentValidation;

namespace Aurum.Api.Features.Journals.Validators;

public sealed class CreateJournalEntryRequestValidator : AbstractValidator<CreateJournalEntryRequest>
{
    private static readonly string[] ValidKinds = { "GJ", "AE", "CE" };

    public CreateJournalEntryRequestValidator()
    {
        RuleFor(x => x.Kind)
            .Must(k => ValidKinds.Contains(k))
            .WithMessage("Kind must be one of: GJ, AE, CE.");

        RuleFor(x => x.Date)
            .InclusiveBetween(1, 31);

        RuleForEach(x => x.Rows).SetValidator(new JournalEntryRowRequestValidator());

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

public sealed class JournalEntryRowRequestValidator : AbstractValidator<JournalEntryRowRequest>
{
    public JournalEntryRowRequestValidator()
    {
        RuleFor(x => x.AccountRef).NotEmpty();
        RuleFor(x => x.AccountName).NotEmpty();
        RuleFor(x => x.Debit).GreaterThanOrEqualTo(0).When(x => x.Debit.HasValue);
        RuleFor(x => x.Credit).GreaterThanOrEqualTo(0).When(x => x.Credit.HasValue);
    }
}
