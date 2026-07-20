# Journals

Ported 1:1 from NestJS's `JournalEntriesModule`. This is the core
bookkeeping engine — every write (create/update/remove) is gated by
`Common/PeriodLock/PeriodLockPolicy.AssertMutable`, exactly where the
original `journal-entries.service.ts` called `assertPeriodMutable`.

## Implemented

- `POST /api/journal-entries` — creates one row per entry line
  (`JournalEntryRowRequest`), all sharing one `groupId`.
- `GET /api/journal-entries?periodId=` — returns `[]` if `periodId` is
  omitted, same as Nest.
- `GET /api/journal-entries/{groupId}` — all rows in the group, 404 if none.
- `PATCH /api/journal-entries/{groupId}` — edits date + all rows in one call;
  rejects any row id that doesn't belong to the group.
- `DELETE /api/journal-entries/{groupId}` — deletes the whole group.
- Balance validation (`Validators/JournalBalanceRules.cs`) ported 1:1 from
  Nest's `IsBalancedJournalConstraint`: total debit must equal total
  credit (±0.005 tolerance), and every row must carry exactly one side.
  One cosmetic difference: error messages format amounts with invariant
  `N2` instead of Nest's `toLocaleString('id-ID')` — the number itself is
  the same, just formatted without Indonesian thousands/decimal separators.

## Database schema

`JournalEntry` maps onto the pre-existing `journal_entries` table — same
columns, same native `journal_kind` enum (GJ/AE/CE). No FK navigation to
`Period` is declared in EF metadata (period_id is a plain column) so EF
never tries to manage that relationship's constraint.

## API contract compatibility

Every response is flat JSON matching Nest's shapes exactly — arrays of raw
rows on success, `{ ok: true }` on delete, `{ error: "..." }` on failure.

## Not yet implemented

Nothing outstanding for this feature — it's a complete port.
