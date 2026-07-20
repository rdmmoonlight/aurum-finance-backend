# Ledger

**No NestJS contract to mirror.** `packages/db/views.ts::queryView` (trial
balance, post-close trial balance, income statement) existed in the
original monorepo but was never called from any controller in the NestJS
backend or anywhere in the Next.js frontend — it was dead code sitting on
top of three real Postgres views. This feature is the first time those
views are exposed over HTTP.

## Implemented

- `GET /api/ledger/trial-balance?periodId=` — from `vw_trial_balance`.
- `GET /api/ledger/trial-balance-post?periodId=` — from `vw_trial_balance_post`.
- `GET /api/ledger/income-statement?periodId=` — from `vw_income_statement`.

Each returns a flat array of `{ accountRef, accountName, totalDebit,
totalCredit, netBalance }` (or `net` instead of `netBalance` for income
statement) — same column selection and `user_id`/`period_id` filter as
`queryView` used, just reachable now.

## Database schema

Read-only. `Views/TrialBalanceRow.cs`, `TrialBalancePostRow.cs`,
`IncomeStatementRow.cs` are keyless EF Core entities (`HasNoKey()` +
`.ToView(...)`) mapped onto the three pre-existing views — no migration is
generated for them, they're query-only.

## Not yet implemented

- No annual-summary-via-view equivalent here — that's still `Features/Reports`,
  which recomputes in application code rather than querying `vw_annual_summary`
  (see that feature's README for the tradeoff note).
