using Aurum.Api.Core.Exceptions;
using Aurum.Api.Features.Accounting.Periods.Entities;
using Aurum.Api.Features.Journals.Entities;

namespace Aurum.Api.Common.PeriodLock;

/// <summary>
/// Ported from NestJS's common/period-lock.ts. That module enforces the
/// closed-book rule in one place instead of re-checking it (inconsistently)
/// per service — this is the same idea, kept as a static domain rule rather
/// than a DI service since it's a pure function of a period's state. Just
/// like the Nest original (which imported JournalKind from the
/// journal-entries feature into common/period-lock.ts), this Common class
/// depends on the Journals feature's JournalKind type.
/// </summary>
public static class PeriodLockPolicy
{
    private static readonly JournalKind[] ClosingKinds = { JournalKind.Ce };

    /// <summary>
    /// Rules:
    ///  - status Closed  → nothing can be created, edited, or deleted.
    ///  - status Closing → only Closing Entries (CE) may be posted/edited/
    ///                      deleted; GJ/AE are frozen.
    ///  - status Open    → GJ/AE are writable; CE is not allowed yet (a
    ///                      closing entry only makes sense once the period
    ///                      has actually started closing).
    /// </summary>
    public static void AssertMutable(Period period, JournalKind kind)
    {
        if (period.Status == PeriodStatus.Closed)
        {
            throw new BadRequestException("This period is closed — no entries can be added, edited, or deleted.");
        }

        if (period.Status == PeriodStatus.Closing && !ClosingKinds.Contains(kind))
        {
            throw new BadRequestException(
                "This period is closing — only closing journal entries (CE) can be posted, edited, or deleted until it's fully closed.");
        }

        if (period.Status == PeriodStatus.Open && ClosingKinds.Contains(kind))
        {
            throw new BadRequestException("Closing journal can only be created when the period status is 'closing'.");
        }
    }

    /// <summary>
    /// The period entity itself (its `my`, seedCash/seedBank, status, etc.)
    /// — once closed, it is view + delete only. No field may be edited,
    /// and that specifically includes reopening it (setting status back to
    /// open/closing is an edit like any other). Deleting a closed period is
    /// still allowed — PeriodsService.RemoveAsync has no equivalent guard.
    /// </summary>
    public static void AssertEditable(Period period)
    {
        if (period.Status == PeriodStatus.Closed)
        {
            throw new BadRequestException("This period is closed — it can only be viewed or deleted, not edited.");
        }
    }
}
