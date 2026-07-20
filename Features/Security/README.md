# Security (Guardian / Health / Audit Log)

Ported 1:1 from NestJS's `SecurityModule` (`SecurityController`,
`AuditLogService`, `HealthService`) — the same three routes Android's
`ApiService.kt` actually calls.

## Implemented

- `GET /api/security/guardian` — full 5-domain weighted health report
  (security 25, integrity 30, health 25, performance 10, configuration 10).
- `GET /api/security/health` — legacy alias, same guardian score with only
  the "security" domain's items.
- `GET /api/security/audit-log` — `{ events, failedCount }` for the caller.
- `IAuditLogService` — in-memory ring buffer (max 500 events), registered
  as a **singleton** (see `SecurityFeatureServiceExtensions`) so it's the
  one shared buffer for the whole process, same as Nest's default-singleton
  `@Injectable()`. One deliberate addition beyond a literal port: a `lock`
  around the buffer, since ASP.NET serves requests on a thread pool (unlike
  Node's single-threaded event loop) and the original had no need for one.

## Content changes from the auth migration

Two places were adapted because Supabase is no longer part of this stack
(see the root README's auth migration note) — everything else is unchanged:

- **Security domain, "Authentication" item**: text changed from "Supabase
  session valid" to "JWT bearer session valid."
- **Health domain**: the "Supabase (Auth)" item became "Authentication
  Provider" (value "JWT (local)"), and the required-env-vars check changed
  from `DATABASE_URL, SUPABASE_URL, SUPABASE_ANON_KEY` to `DATABASE_URL,
  JWT_SIGNING_KEY` — matching `Core/Extensions/AuthenticationServiceExtensions.cs`
  and `.env.example`.
- **Configuration domain**: same env var list change as above.
- `AppVersion` is read from configuration (`appsettings.json`, key
  `AppVersion`) instead of Nest's `version.json` — bump it manually the
  same way `version.json` was maintained.

## Not yet implemented

- `Core/Middleware/AuditLoggingMiddleware.cs` now covers the original
  `AuditInterceptor`'s job (logs every request under category `DATA`,
  wired in `Program.cs` after `UseAuthorization()`). What's still missing:
  `SupabaseAuthMiddleware`'s separate auth-failure logging under category
  `AUTH` — that would mean hooking `JwtBearerEvents.OnAuthenticationFailed`
  in `AuthenticationServiceExtensions.cs` to call `IAuditLogService.Record`
  for invalid/missing tokens specifically.
