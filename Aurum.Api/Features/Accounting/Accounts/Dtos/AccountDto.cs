namespace Aurum.Api.Features.Accounting.Accounts.Dtos;

/// <summary>
/// Field names/casing match exactly what the NestJS backend returned (a raw
/// Drizzle row serialized as camelCase JSON): id, userId, ref, name, role,
/// isActive, sortOrder, createdAt, updatedAt.
/// </summary>
public sealed class AccountDto
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public string Ref { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Role { get; init; }

    public bool IsActive { get; init; }

    public int SortOrder { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
