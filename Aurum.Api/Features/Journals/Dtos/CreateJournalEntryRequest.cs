namespace Aurum.Api.Features.Journals.Dtos;

public sealed class CreateJournalEntryRequest
{
    public Guid PeriodId { get; init; }

    /// <summary>"GJ", "AE", or "CE".</summary>
    public string Kind { get; init; } = string.Empty;

    public Guid GroupId { get; init; }

    /// <summary>Day-of-month, 1-31.</summary>
    public int Date { get; init; }

    public List<JournalEntryRowRequest> Rows { get; init; } = new();
}
