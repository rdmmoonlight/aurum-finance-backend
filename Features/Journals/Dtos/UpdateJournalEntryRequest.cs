namespace Aurum.Api.Features.Journals.Dtos;

public sealed class UpdateJournalEntryRowRequest : JournalEntryRowRequest
{
    public Guid Id { get; init; }
}

/// <summary>
/// Body for PATCH /journal-entries/{groupId} — edits an entire transaction
/// group in one shot (date + all its rows). Same balance gateway as create.
/// </summary>
public sealed class UpdateJournalEntryRequest
{
    public int Date { get; init; }

    public List<UpdateJournalEntryRowRequest> Rows { get; init; } = new();
}
