# Authentication

Owns credential storage and JWT issuing directly: register/login, refresh
tokens, password reset, email verification, and role-based authorization.
This is the only feature in the API today — see the root README for what
else is planned.

## Implemented

- `POST /api/auth/register` — creates a `User` (see `Features/Users`),
  hashes the password with `IPasswordHasher<User>`, issues an email
  verification token (sent via `IEmailSender`), returns the new user's
  `AuthResponse` directly (HTTP 201, no envelope).
- `POST /api/auth/login` — verifies credentials, enforces account lockout
  after repeated failures, returns `AuthResponse` directly (HTTP 200, no
  envelope).
- `POST /api/auth/refresh` — exchanges a still-valid refresh token for a
  new access token, rotating the refresh token in the same call (old one is
  revoked, a new one is issued).
- `POST /api/auth/logout` — revokes a single refresh token.
- `GET /api/auth/me` — `[Authorize]`-protected, returns `AuthenticatedUserDto`
  directly via `ICurrentUserService`.
- `POST /api/auth/forgot-password` — always returns 204 whether or not the
  email is registered; issues a password reset token and emails it via
  `IEmailSender` if the account exists and is active.
- `POST /api/auth/reset-password` — consumes a password reset token, sets
  the new password, and revokes every refresh token for that user (a reset
  ends every existing session, not just the one that requested it).
- `POST /api/auth/verify-email` — consumes an email verification token and
  marks the account's email as confirmed.
- `POST /api/auth/resend-verification` — always returns 204; issues a new
  verification token if the account exists and isn't already verified.
- JWT issuing (`Infrastructure/Security/JwtTokenService`) and validation
  (JwtBearer, wired in `Core/Extensions/AuthenticationServiceExtensions.cs`)
  are HS256, claims: `sub` (user id), `email`, `role` (as `ClaimTypes.Role`,
  so `[Authorize(Roles = "Admin")]` works on any controller with no extra
  wiring).
- Refresh tokens (`Infrastructure/Security/IRefreshTokenService`) are opaque
  random strings, not JWTs — only their SHA-256 hash is stored, in
  `refresh_tokens`. Password reset and email verification tokens follow the
  same "store the hash, not the secret" pattern in their own tables.
- Account lockout: `User.FailedLoginAttempts` increments on every failed
  login; once it reaches `Auth:MaxFailedLoginAttempts` (default 5),
  `User.LockedUntilUtc` is set `Auth:LockoutMinutes` (default 15) into the
  future and login is rejected until it passes. Resets to 0 on any
  successful login or password reset.
- Role-based authorization: `User.Role` is a single `UserRole` value
  (`User` or `Admin`), stored as text (not a native Postgres enum, so
  adding it is a plain `ALTER TABLE ADD COLUMN`, no `CREATE TYPE`).

## API contract

Every response — success or error — is returned **flat, with no wrapper**:
a bare object on success, `{ "error": "message" }` on failure (see
`Core/Middleware/ExceptionHandlingMiddleware.cs`). Failed
`[Authorize(Roles = ...)]` checks are also routed through this same shape
(HTTP 403) via
`Infrastructure/Security/AppAuthorizationMiddlewareResultHandler.cs`,
instead of ASP.NET's default empty 403 body.

## Database schema

`users`, `refresh_tokens`, `email_verification_tokens`, and
`password_reset_tokens` are the entire schema today, created by
`dotnet ef database update` against a connection string set via the
`DATABASE_URL` environment variable (see
`Core/Extensions/DatabaseServiceExtensions.cs`). Never commit a real
connection string or JWT signing key to source control; set both as
environment variables (locally, in `.env`, or via `dotnet user-secrets`)
instead.

## Email delivery

`Infrastructure/Email/IEmailSender` is the abstraction every caller depends
on. The registered default, `LoggingEmailSender`, writes the message to the
application log instead of sending it through a paid provider — this keeps
register/forgot-password/resend-verification fully working with zero
billing dependency. Swap the registration in
`Core/Extensions/SecurityServiceExtensions.cs` for a real
SMTP/API-based implementation when one is chosen; no other code needs to
change.

## Not yet implemented

- No admin-only endpoint exists yet to exercise `[Authorize(Roles = "Admin")]`
  end-to-end — the claim and the attribute both work, but nothing in this
  API currently requires the Admin role. Add `[Authorize(Roles = "Admin")]`
  to any controller action that should be Admin-only.

## Configuration

`appsettings.json`'s `Jwt:SigningKey` and `ConnectionStrings:DefaultConnection`
are deliberately left empty — both must be supplied as environment
variables (`JWT_SIGNING_KEY`, `DATABASE_URL`), never committed to source
control. See `.env.example` at the repo root for every variable this
feature reads, plus the `Jwt:*` / `Auth:*` sections in `appsettings.json`
for the non-secret defaults (token lifetimes, lockout thresholds).
