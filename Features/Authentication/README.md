# Authentication

Migrated from NestJS's `SupabaseAuthMiddleware`. That middleware only
*verified* tokens Supabase had already issued; this feature now owns
credential storage and token issuing directly (no more Supabase dependency
for auth) — register/login/JWT generation, refresh tokens, password reset,
email verification, and role-based authorization are all responsibilities
this API now has that it didn't before.

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

## API contract compatibility

Every response — success or error — is returned **flat, with no wrapper**,
matching how the NestJS backend responded (a bare array/object on success,
`{ "error": "message" }` on failure via its global `HttpExceptionFilter`).
Failed `[Authorize(Roles = ...)]` checks are also routed through this same
shape (HTTP 403) via `Infrastructure/Security/AppAuthorizationMiddlewareResultHandler.cs`,
instead of ASP.NET's default empty 403 body.

`AuthResponse` gained a `RefreshToken` field and `AuthenticatedUserDto`
gained `Role` / `IsActive` / `EmailConfirmed` fields — both are additive,
so a frontend that doesn't read the new fields yet keeps working unchanged.
`AuthenticatedUserDto.DisplayName` and `RegisterRequest.DisplayName` were
renamed to `FullName` to match the target schema — this **is** a breaking
field-name change for any caller still sending/reading `displayName`.

## Database schema

`users` gained new columns for this pass: `full_name` (renamed from
`display_name`), `role`, `is_active`, `email_confirmed_at_utc`,
`last_login_at_utc`, `failed_login_attempts`, `locked_until_utc`. Three new
tables were added: `refresh_tokens`, `email_verification_tokens`,
`password_reset_tokens` — each stores only a token hash, never the raw
token. No existing table besides `users` is touched. Run
`dotnet ef migrations add AddAuthHardening` and review the generated
migration before applying it — see the doc comment on `AppDbContext` for
specifics.

To avoid a "big migration" of existing data: every other table
(`accounts`, `periods`, `journal_entries`, `bank_accounts`,
`bank_transactions`) stores a `user_id uuid` that was previously a Supabase
auth user id. When backfilling the `users` table from existing Supabase
accounts, **reuse the same UUIDs** as the new `users.id` (don't generate
fresh ones) — that's what keeps every existing row in those tables joined
correctly with zero changes to them.

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

- The Supabase → local `users` backfill script described above.
- No admin-only endpoint exists yet to exercise `[Authorize(Roles = "Admin")]`
  end-to-end — the claim and the attribute both work, but nothing in this
  API currently requires the Admin role. Add `[Authorize(Roles = "Admin")]`
  to any controller action that should be Admin-only.

## Configuration

See `.env.example` at the repo root for `JWT_SIGNING_KEY` and the optional
`JWT__ISSUER` / `JWT__AUDIENCE` / `JWT__ACCESSTOKENEXPIRYMINUTES` /
`JWT__REFRESHTOKENEXPIRYDAYS` overrides, plus the new `Auth:*` section
(`MaxFailedLoginAttempts`, `LockoutMinutes`,
`EmailVerificationTokenExpiryHours`, `PasswordResetTokenExpiryMinutes`) in
`appsettings.json`. `appsettings.Development.json` ships a dev-only signing
key so `dotnet run` works without any setup — never reuse it outside local
development.
