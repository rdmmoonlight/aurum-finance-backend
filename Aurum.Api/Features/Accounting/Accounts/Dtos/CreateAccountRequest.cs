namespace Aurum.Api.Features.Accounting.Accounts.Dtos;

public sealed class CreateAccountRequest
{
    public string Ref { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    /// <summary>Only "clearing" is a real role today; null means a normal account.</summary>
    public string? Role { get; init; }

    public int? SortOrder { get; init; }
}
