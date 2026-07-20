namespace Aurum.Api.Features.Journals.Dtos;

public sealed class JournalEntryDto
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public Guid PeriodId { get; init; }

    public string Kind { get; init; } = string.Empty;

    public Guid GroupId { get; init; }

    public int Date { get; init; }

    public string AccountRef { get; init; } = string.Empty;

    public string AccountName { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public decimal? Debit { get; init; }

    public decimal? Credit { get; init; }

    public bool IsClosingTemp { get; init; }

    public int SortOrder { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
