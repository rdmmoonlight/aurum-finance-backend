namespace Aurum.Api.Features.Reports;

/// <summary>
/// Ported 1:1 from NestJS's annual-summary.service.ts::classFromRef, which
/// itself mirrors apps/web/src/types/accounting.ts. Derives an account
/// class from the numeric ref range. Keep the ranges identical to both of
/// those sources if either ever changes.
/// </summary>
public static class AccountClassifier
{
    /// <summary>Returns "A", "L", "E", "R", "EXP", "OR", "OE", or null if the ref isn't numeric/in range.</summary>
    public static string? ClassFromRef(string accountRef)
    {
        if (!int.TryParse(accountRef, out var n))
        {
            return null;
        }

        return n switch
        {
            >= 101 and <= 199 => "A",
            >= 200 and <= 299 => "L",
            >= 300 and <= 399 => "E",
            >= 400 and <= 499 => "R",
            >= 500 and <= 799 => "EXP",
            >= 800 and <= 899 => "OR",
            >= 900 and <= 999 => "OE",
            _ => null,
        };
    }
}
