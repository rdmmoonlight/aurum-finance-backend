namespace Aurum.Api.Features.Accounting.Accounts.Entities;

/// <summary>
/// Maps onto the existing "accounts" table (created by the original
/// Drizzle/NestJS schema) — no schema change. See
/// Configurations/AccountConfiguration.cs for exact column mapping.
/// </summary>
public sealed class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    /// <summary>3-4 numeric digits, e.g. "101", "5001" — enforced by chk_ref_numeric in the DB.</summary>
    public string Ref { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public AccountRole? Role { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
