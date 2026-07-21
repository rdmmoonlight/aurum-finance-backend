namespace Aurum.Api.Features.Users.Entities;

/// <summary>
/// Single-role authorization model. Stored as a string column (not a native
/// Postgres enum) so adding this column never requires a CREATE TYPE
/// migration — only an ALTER TABLE ADD COLUMN. Emitted into the JWT as a
/// standard ClaimTypes.Role claim, so built-in [Authorize(Roles = "Admin")]
/// attributes work with no extra wiring.
/// </summary>
public enum UserRole
{
    User = 0,
    Admin = 1,
}
