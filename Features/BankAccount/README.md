# BankAccount (BRI integration)

Ported 1:1 from NestJS's `BankAccountModule`. The BRI client itself lives
in `Infrastructure/External/Bri/` rather than inside this feature folder —
a deliberate placement (per the migration plan's "third-party integration
→ Infrastructure/External" step), not an oversight.

## Implemented

- `GET /api/bank-account` — `{ isLinked, account, transactions }`.
- `POST /api/bank-account/link` — verifies via `IBriClient.InquireAccountAsync`,
  links/relinks (one account per user), then pulls initial mutations.
- `POST /api/bank-account/sync` — refreshes balance + mutations for the
  already-linked account.
- `PATCH /api/bank-account/transactions/{id}` — sets category/notes for
  reconciliation; uses `Optional<T>` so omitted vs. explicit-null is
  distinguishable, same convention as every other Update DTO in this API.

## Infrastructure/External/Bri

- `IBriClient` / `BriClient` — OAuth2 `client_credentials` token exchange
  and `BRI-Signature` HMAC-SHA256 request signing are **real and wired
  up**, matching BRI's public docs exactly like the Nest original.
- `InquireAccountAsync` / `FetchMutationsAsync` are **still placeholders**
  — same as Nest: the actual balance/mutation business endpoints aren't
  called yet because BRIAPI splits that functionality across several
  products (Account Information, Fund Transfer, SNAP, BRIVA, ...) and it
  isn't yet confirmed which one this Client ID is provisioned for. Until
  then, credentials are verified (a real token exchange happens) but the
  returned balance/mutations are deterministic simulated data — identical
  formulas to the Nest version, so a given account number always produces
  the same simulated numbers on both backends.
- Registered as a **singleton** (caches the OAuth2 token in memory across
  requests, like Nest's default `@Injectable()` scope) but uses
  `IHttpClientFactory` (a named `"Bri"` client) instead of a raw
  `new HttpClient()` per call, to avoid socket exhaustion under ASP.NET's
  threaded request model — the one infrastructure difference from a
  literal port. A `SemaphoreSlim` guards concurrent token refreshes for
  the same reason (Node's single-threaded model didn't need one).

## Database schema

`LinkedBankAccount` and `BankTransaction` map onto the pre-existing
`bank_accounts` and `bank_transactions` tables — same columns, same native
enums (`bank_provider`, `bank_transaction_type`). No FK navigation between
them is declared in EF metadata (same reasoning as `JournalEntry`↔`Period`).

## Configuration

`BRI_BASE_URL` / `BRI_CLIENT_ID` / `BRI_CLIENT_SECRET` — see `.env.example`.
All three must be set for `IBriClient.IsConfigured` to be true; if any are
missing, every call silently falls back to simulated data (matching Nest's
behavior exactly, including in local development with no BRI credentials
at all).

## API contract compatibility

Every response is flat JSON matching Nest's shapes exactly.

## Not yet implemented

- The real BRI balance/mutation endpoints — see the TODO comments in
  `BriClient.cs` for exactly what's needed once the BRI product is confirmed.
