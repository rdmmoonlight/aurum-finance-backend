# Authentication

Migrated from NestJS's `SupabaseAuthMiddleware`. That middleware only
*verified* tokens Supabase had already issued; this feature now owns
credential storage and token issuing directly (no more Supabase dependency
for auth) — register/login/JWT generation are new responsibilities this
API didn't have before.

## Implemented

- `POST /api/auth/register` — creates a `User` (see `Features/Users`),
  hashes the password with `IPasswordHasher<User>`, returns the new user's
  `AuthResponse` directly (HTTP 201, no envelope).
- `POST /api/auth/login` — verifies credentials, returns `AuthResponse`
  directly (HTTP 200, no envelope).
- `GET /api/auth/me` — `[Authorize]`-protected, returns `AuthenticatedUserDto`
  directly via `ICurrentUserService`.
- JWT issuing (`Infrastructure/Security/JwtTokenService`) and validation
  (JwtBearer, wired in `Core/Extensions/AuthenticationServiceExtensions.cs`)
  are HS256, claims: `sub` (user id), `email`.

## API contract compatibility

Every response — success or error — is returned **flat, with no wrapper**,
matching how the NestJS backend responded (a bare array/object on success,
`{ "error": "message" }` on failure via its global `HttpExceptionFilter`).
This project's original scaffold included an `ApiResponse<T>` /
`ApiErrorResponse` envelope; both were **removed** because they would have
broken frontend compatibility. See `Core/Shared/ErrorResponse.cs` and
`Core/Middleware/ExceptionHandlingMiddleware.cs`.

These `/api/auth/*` routes are new (Supabase handled login/register
client-side before, so there's no prior Nest contract to mirror for them
specifically) — but they follow the same flat convention as every other
(migrated) endpoint for consistency, and so the frontend's response-parsing
logic doesn't need two different shapes depending on which endpoint it calls.

## Database schema

No existing table is touched. `Features/Users/Entities/User.cs` maps to a
**net-new** `users` table — the previous stack had no local `users` table at
all; user records lived entirely in Supabase's own `auth.users` schema,
external to this database.

To avoid a "big migration" of existing data: every other table
(`accounts`, `periods`, `journal_entries`, `bank_accounts`,
`bank_transactions`) stores a `user_id uuid` that was previously a Supabase
auth user id. When backfilling the new `users` table from existing Supabase
accounts, **reuse the same UUIDs** as the new `users.id` (don't generate
fresh ones) — that's what keeps every existing row in those tables joined
correctly with zero changes to them. A backfill script (export Supabase
`auth.users` → insert into this `users` table with the same id + email, and
a placeholder/must-reset password since Supabase password hashes aren't
portable) is a one-time, additive operation — not a migration of the
accounting data itself.

## Not yet implemented

- Refresh tokens / token revocation — access tokens are short-lived
  (`Jwt:AccessTokenExpiryMinutes`, default 60) and there is no refresh flow
  yet. Re-login is currently the only way to get a new token once one expires.
- Email verification, password reset.
- Role/claims-based authorization — every authenticated user currently has
  equal access; there's no admin/owner distinction.
- The Supabase → local `users` backfill script described above.

## Configuration

See `.env.example` at the repo root for `JWT_SIGNING_KEY` and the optional
`JWT__ISSUER` / `JWT__AUDIENCE` / `JWT__ACCESSTOKENEXPIRYMINUTES` overrides.
`appsettings.Development.json` ships a dev-only signing key so `dotnet run`
works without any setup — never reuse it outside local development.
