using System.Text.RegularExpressions;
using Aurum.Api.Features.Accounting.Periods.Dtos;
using FluentValidation;

namespace Aurum.Api.Features.Accounting.Periods.Validators;

public sealed partial class UpdatePeriodRequestValidator : AbstractValidator<UpdatePeriodRequest>
{
    [GeneratedRegex(@"^(0[1-9]|1[0-2])-\d{4}$")]
    private static partial Regex MyPattern();

    private static readonly string[] ValidStatuses = { "open", "closing", "closed" };

    public UpdatePeriodRequestValidator()
    {
        RuleFor(x => x.My.Value!)
            .Must(my => MyPattern().IsMatch(my))
            .WithMessage("Period must be in MM-YYYY format (e.g. \"06-2026\").")
            .When(x => x.My.IsSet);

        RuleFor(x => x.SeedCash.Value)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SeedCash.IsSet);

        RuleFor(x => x.SeedBank.Value)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SeedBank.IsSet);

        RuleFor(x => x.Status.Value!)
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage("Status must be one of: open, closing, closed.")
            .When(x => x.Status.IsSet);

        RuleFor(x => x.ClosedAt.Value!)
            .Must(v => DateTime.TryParse(v, out _))
            .WithMessage("closedAt must be a valid ISO 8601 timestamp.")
            .When(x => x.ClosedAt.IsSet);
    }
}
