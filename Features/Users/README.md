# Users

Holds the `User` entity that the Authentication feature registers,
authenticates, and issues tokens for: id, email, password hash, full name,
role, active flag, email-confirmation timestamp, last-login timestamp, and
failed-login/lockout tracking. Split from `Features/Authentication` because
"the user record" and "the act of logging in" are different concerns —
later user-profile endpoints (update full name, change password, delete
account, admin user management) belong here, not in Authentication.

## Implemented

- `Entities/User.cs` — the entity, mapped via `Configurations/UserConfiguration.cs`
  (unique index on `email`).
- `Entities/UserRole.cs` — `User` / `Admin`, stored as text on `User.Role`
  and emitted into the JWT as a `ClaimTypes.Role` claim by
  `JwtTokenService`, so `[Authorize(Roles = "Admin")]` works anywhere in
  the API.

## Not yet implemented

- No endpoints of its own yet — `Features/Authentication/AuthController.cs`
  is currently the only consumer (`GET /api/auth/me`). Profile management
  endpoints (update name, change password, delete account) and an
  admin-only user-management endpoint would live here when needed.
