using Aurum.Api.Core.Shared;

namespace Aurum.Api.Features.Accounting.Accounts.Dtos;

/// <summary>
/// Mirrors NestJS's UpdateAccountDto (PartialType(CreateAccountDto)) — every
/// field optional. Uses Optional&lt;T&gt; instead of plain nullable
/// properties so "field omitted" (leave unchanged) and "field explicitly
/// null" (e.g. clear Role) are distinguishable — see Core/Shared/Optional.cs.
/// </summary>
public sealed class UpdateAccountRequest
{
    public Optional<string> Ref { get; init; }

    public Optional<string> Name { get; init; }

    public Optional<string?> Role { get; init; }

    public Optional<int> SortOrder { get; init; }
}
