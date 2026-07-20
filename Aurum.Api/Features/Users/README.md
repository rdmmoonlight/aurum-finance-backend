# Users

Holds the `User` entity (id, email, password hash, display name) that the
Authentication feature registers, authenticates, and issues tokens for.
Split from `Features/Authentication` because "the user record" and "the act
of logging in" are different concerns — later user-profile endpoints
(update display name, change password, delete account) belong here, not in
Authentication.

## Implemented

- `Entities/User.cs` — the entity, mapped via `Configurations/UserConfiguration.cs`
  (unique index on `email`).

## Not yet implemented

- No endpoints of its own yet — `Features/Authentication/AuthController.cs`
  is currently the only consumer (`GET /api/auth/me`). Profile management
  endpoints (update name, change password, delete account) would live here
  when needed.
