# Dashboard

Confirmed empty on purpose — mirrors NestJS's `DashboardModule` exactly:
a controller shell with **zero routes** (`backend/src/dashboard/dashboard.controller.ts`
had no `@Get`/`@Post` methods at all). There's nothing to port; this isn't
a gap, it's a faithful copy of "nothing here yet" from the source.

If/when this feature gets real requirements, migrate it the same way as
every other feature here: entities (if any) mapped onto existing tables
per the root README's schema-preservation rule, DTOs matching whatever
contract the frontend needs, flat JSON responses with no wrapper.
