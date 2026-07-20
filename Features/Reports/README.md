# Reports (Annual Summary)

Ported 1:1 from NestJS's `AnnualSummaryModule`.

## Implemented

- `GET /api/annual-summary?year=YYYY` — monthly revenue/expense/profit/loss
  breakdown for the given year, plus year totals.
- `AccountClassifier.ClassFromRef` ports Nest's `classFromRef` (numeric ref
  range → A/L/E/R/EXP/OR/OE), kept in sync with the same ranges as the
  frontend's `apps/web/src/types/accounting.ts` and the Accounting feature.
- Closing Entries (`kind = CE`) are skipped during aggregation to avoid
  double-counting — same as Nest.

## Database schema note

The pre-existing `vw_annual_summary` Postgres view already computes this
same aggregation server-side, but this port — like the original NestJS
service — recomputes it in application code instead of querying the view.
If you want to switch to the view later: it would mean adding a keyless
EF Core entity mapped with `.ToView("vw_annual_summary")` and querying that
instead of `AnnualSummaryService`'s current in-process loop. Not done here
to keep this a faithful 1:1 port first; revisit once the whole API is
migrated and the view's exact output shape has been verified to match.

## API contract compatibility

Response shape matches Nest's exactly: `{ year, months, totals, periods }`,
flat, no wrapper.
