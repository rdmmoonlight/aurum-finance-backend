using System.Text.RegularExpressions;
using Aurum.Api.Features.Accounting.Periods.Dtos;
using FluentValidation;

namespace Aurum.Api.Features.Accounting.Periods.Validators;

public sealed partial class CreatePeriodRequestValidator : AbstractValidator<CreatePeriodRequest>
{
    [GeneratedRegex(@"^(0[1-9]|1[0-2])-\d{4}$")]
    private static partial Regex MyPattern();

    private static readonly string[] ValidStatuses = { "open", "closing", "closed" };

    public CreatePeriodRequestValidator()
    {
        RuleFor(x => x.My)
            .Must(my => MyPattern().IsMatch(my))
            .WithMessage("Period must be in MM-YYYY format (e.g. \"06-2026\").");

        RuleFor(x => x.SeedCash)
            .GreaterThanOrEqualTo(0)
            .Must(HaveAtMostTwoDecimalPlaces);

        RuleFor(x => x.SeedBank)
            .GreaterThanOrEqualTo(0)
            .Must(HaveAtMostTwoDecimalPlaces);

        RuleFor(x => x.Status)
            .Must(s => s is null || ValidStatuses.Contains(s))
            .WithMessage("Status must be one of: open, closing, closed.");
    }

    private static bool HaveAtMostTwoDecimalPlaces(decimal value) => decimal.Round(value, 2) == value;
}
