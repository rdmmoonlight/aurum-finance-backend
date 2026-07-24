using Microsoft.AspNetCore.Identity;

namespace Aurum.Api.Features.Users.Entities;

/// <summary>
/// The application's user type, backed entirely by ASP.NET Core Identity.
/// All credential storage, password hashing, lockout, and token issuance
/// (login, refresh, email confirmation, password reset) is owned by
/// Identity itself — this class only adds the domain-specific profile
/// fields Identity doesn't provide out of the box.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string? FullName { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
