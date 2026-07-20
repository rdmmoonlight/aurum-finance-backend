# Accounting (Accounts + Periods)

Ported 1:1 from NestJS's `AccountsModule` and `PeriodsModule`. Split into two
sub-namespaces here (`Accounting/Accounts`, `Accounting/Periods`) matching
the two separate Nest modules, both under one `Features/Accounting/` folder
since that's the placeholder name this scaffold already had.

## Implemented

### Accounts — `/api/accounts`
- `GET /`, `POST /`, `PATCH /reorder`, `POST /reset`, `PATCH /{id}`, `DELETE /{id}`
- Business rules ported: unique `(userId, ref)`, block delete when an
  account has journal entries, block reset when the given active period
  already has transactions, hard-reset seeds from `DefaultAccounts` (kept
  byte-for-byte identical to Nest's `default-accounts.ts`).
- **Route order preserved**: `reorder` and `reset` are registered before
  `{id}` — same trap Nest's own source comment warned about.

### Periods — `/api/periods`
- `POST /`, `GET /`, `GET /{id}`, `PATCH /{id}`, `DELETE /{id}`
- Business rules ported: unique `(userId, my)`, `seedCash + seedBank > 0` on
  create, closed periods are view/delete-only (`Common/PeriodLock/PeriodLockPolicy.AssertEditable`).

## Database schema

Both `Account` and `Period` map onto the **pre-existing** `accounts` and
`periods` tables (created by the original Drizzle schema) — same table/column
names, same native Postgres enums (`account_role`, `period_status`, mapped
via `NpgsqlDataSourceBuilder.MapEnum` in `DatabaseServiceExtensions`). No
`CREATE TABLE`/`ALTER TABLE` migration should be generated for these two
entities — see the root README's "Same database schema" note.

`journal_entries` existence checks (Accounts' delete-guard and reset-guard)
use EF LINQ against the real `JournalEntry` entity now that the Journals
feature is migrated (`_db.JournalEntries.AnyAsync(...)`) — these started as
raw `SqlQuery<int>` calls when Accounting was migrated before Journals
existed; both were replaced once the entity was available.

## API contract compatibility

Every response is flat JSON matching the original Nest response shapes
exactly (raw arrays/objects, `{ ok: true }` on delete, `{ error: "..." }` on
failure) — see the root README's "Same API contract" note.

## Not yet implemented

Nothing outstanding for Accounts/Periods themselves. `PeriodLockPolicy.AssertMutable`
(the per-journal-kind rule) is now implemented too, in `Common/PeriodLock/`,
since the Journals feature that needs it has been migrated.
