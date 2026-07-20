namespace Aurum.Api.Features.Journals.Dtos;

public class JournalEntryRowRequest
{
    public string AccountRef { get; init; } = string.Empty;

    public string AccountName { get; init; } = string.Empty;

    public string? Description { get; init; }

    public decimal? Debit { get; init; }

    public decimal? Credit { get; init; }
}
