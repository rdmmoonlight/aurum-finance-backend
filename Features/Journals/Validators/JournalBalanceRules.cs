namespace Aurum.Api.Features.Journals.Validators;

/// <summary>
/// Ported 1:1 from NestJS's IsBalancedJournalConstraint. Core accounting
/// rule: a journal entry is only valid if total debits equal total
/// credits, and every row carries exactly one side (never both, never
/// neither). Shared between Create and Update validators, same as the one
/// Nest constraint class was reused across both DTOs via @Validate(...).
/// </summary>
public static class JournalBalanceRules
{
    // Tolerance for floating point rounding on numeric(18,2) amounts.
    private const decimal Epsilon = 0.005m;

    public static bool IsBalanced(IReadOnlyList<(decimal? Debit, decimal? Credit)> rows)
    {
        if (rows.Count < 2)
        {
            return false;
        }

        decimal totalDebit = 0;
        decimal totalCredit = 0;

        foreach (var (debitRaw, creditRaw) in rows)
        {
            var debit = debitRaw ?? 0;
            var credit = creditRaw ?? 0;

            if (debit < 0 || credit < 0)
            {
                return false;
            }

            if (debit > 0 && credit > 0)
            {
                return false; // one side per row only
            }

            if (debit == 0 && credit == 0)
            {
                return false; // every row needs an amount
            }

            totalDebit += debit;
            totalCredit += credit;
        }

        return Math.Abs(totalDebit - totalCredit) < Epsilon;
    }

    public static string BuildMessage(IReadOnlyList<(decimal? Debit, decimal? Credit)> rows)
    {
        if (rows.Count < 2)
        {
            return "A journal entry needs at least two rows (one debit side, one credit side).";
        }

        var totalDebit = rows.Sum(r => r.Debit ?? 0);
        var totalCredit = rows.Sum(r => r.Credit ?? 0);

        if (Math.Abs(totalDebit - totalCredit) >= Epsilon)
        {
            // Note: the original Nest message formats amounts with
            // toLocaleString('id-ID'); here we use invariant "N2" instead.
            // Only the displayed number formatting differs — the message
            // still communicates the same mismatch.
            return $"Journal entry is not balanced: total debit {totalDebit:N2} does not equal total credit {totalCredit:N2}.";
        }

        return "Each row must have exactly one of debit or credit filled in, never both, never neither.";
    }
}
