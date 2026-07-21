# Aurum API

Backend for the Aurum personal finance application, built with ASP.NET Core (C#) using a
modular, feature-based architecture inspired by Clean Architecture and Vertical Slice
Architecture.

## Architecture

```
Aurum.Api
├── Features/            Vertical slices — one folder per business capability
│   ├── Authentication/
│   ├── Users/
│   ├── Accounting/
│   ├── Journals/
│   ├── Ledger/
│   ├── Dashboard/
│   └── Reports/
├── Core/                 Cross-cutting application concerns
│   ├── Middleware/        Global exception handling, etc.
│   ├── Extensions/        DI wiring (Swagger, CORS, rate limiting, versioning, DB, health)
│   ├── Exceptions/        AppException hierarchy → consistent HTTP error responses
│   ├── Shared/             ApiResponse<T> / ApiErrorResponse envelopes
│   └── Utilities/          Stateless helpers (e.g. connection string parsing)
├── Infrastructure/       Technical concerns, isolated from business logic
│   ├── Database/           EF Core AppDbContext (PostgreSQL via Npgsql)
│   ├── Logging/            Serilog configuration
│   ├── Security/           (placeholder) JWT/auth infrastructure
│   └── External/           (placeholder) third-party integrations
├── Contracts/            Shared DTOs used across more than one feature
├── Program.cs
└── appsettings.json
```

**Rules this project follows:**
- Each feature is self-contained; business logic never lives in `Infrastructure/`.
- `AppDbContext` is used directly — no repository abstraction until one earns its keep.
- The structure is CQRS-ready (a feature can add its own `Commands/`/`Queries/` folders),
  but MediatR is intentionally **not** included yet — add it only when a feature actually
  needs command/query separation.
- Migration status: **complete**. Every NestJS module has a counterpart
  here: **Authentication**, **Users**, **Accounting** (Accounts + Periods),
  **Journals**, **Reports** (Annual Summary), **Ledger**, **Security**
  (Guardian/Health/Audit Log), and **BankAccount** (BRI integration, client
  in `Infrastructure/External/Bri/`). **Dashboard** is confirmed empty on
  purpose (mirrors the NestJS source exactly — see its README). See each
  feature's own README for what's a literal 1:1 port vs. what was
  necessarily new (Authentication's endpoints, Ledger's endpoints) vs. what
  was deliberately adapted (Security's Supabase→JWT content changes).
- **Known follow-ups, not blocking**: no `dotnet build`/`dotnet ef
  migrations add` has been run against this code yet (no .NET SDK was
  available in the environment that produced it) — build and generate the
  initial `users`-only migration locally before deploying. The
  Supabase→local `users` backfill script (see Authentication's README) and
  the real BRI balance/mutation endpoints (see BankAccount's README) are
  the two genuine pieces of unfinished work; everything else is complete.
- **Two hard constraints govern every feature migrated from the NestJS backend:**
  1. *Same database schema.* The existing PostgreSQL tables (`accounts`,
     `periods`, `journal_entries`, `bank_accounts`, `bank_transactions` — see
     the original `packages/db/schema.ts`) are **not** altered: same table
     names, column names, types, and constraints. EF Core entity
     configurations for these tables map onto what already exists; they
     don't generate `CREATE TABLE`/`ALTER TABLE` migrations for them. Only
     genuinely new tables (like `users`, added by the Authentication
     feature) get real EF Core migrations. When adding a feature whose
     entities include pre-existing tables, strip the auto-generated
     `CreateTable` calls for those tables out of the migration before
     applying it — see each feature's README for specifics once it's added.
  2. *Same API contract.* Every response — success or error — is returned
     flat, with no wrapper, matching exactly what the NestJS backend
     returned (a bare array/object on success, `{ "error": "message" }` on
     failure). This is why the original `ApiResponse<T>`/`ApiErrorResponse`
     scaffold classes were removed — they would have required the Next.js
     frontend to change its response parsing, not just its base URL. See
     `Core/Shared/ErrorResponse.cs` and `Core/Middleware/ExceptionHandlingMiddleware.cs`.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A PostgreSQL database (local Postgres for development, [Neon](https://neon.tech) in production)
- Docker (optional locally, required for the Render deployment path)

## Local setup

1. **Clone and restore**

   ```bash
   git clone https://github.com/rdmmoonlight/aurum-finance-backend.git
   cd aurum-finance-backend
   dotnet restore
   ```

2. **Configure the database connection**

   Copy `.env.example` (repository root) to `.env` and adjust as needed, or edit
   `appsettings.Development.json` directly. The default assumes a local Postgres instance:

   ```
   Host=localhost;Port=5432;Database=aurum_dev;Username=postgres;Password=postgres
   ```

3. **Run the API**

   ```bash
   dotnet run
   ```

   The API starts at `http://localhost:5080` with Swagger UI at `/swagger`.
   Health checks are available at `/health`.

4. **Apply EF Core migrations** (once the first migration is added)

   ```bash
   dotnet ef database update
   ```

## Configuration reference

| Setting                         | Environment variable            | Purpose                                   |
|----------------------------------|----------------------------------|--------------------------------------------|
| `ConnectionStrings:DefaultConnection` | `DATABASE_URL`              | PostgreSQL connection (URL or keyword=value) |
| `Cors:AllowedOrigins`            | `ALLOWED_ORIGINS` (comma-separated) | Origins allowed to call the API        |
| `RateLimiting:PermitLimit`       | `RATELIMITING__PERMITLIMIT`      | Requests allowed per window, per client IP |
| `RateLimiting:WindowSeconds`     | `RATELIMITING__WINDOWSECONDS`    | Length of the rate limit window in seconds |
| —                                | `PORT`                           | Port Kestrel binds to inside the container |

Environment variables always take priority over `appsettings*.json` values, which is the
convention used for Render deployment.

## Deploying to Render.com

This repository includes a production `Dockerfile` at the repository root, so Render can
build and run the API as a **Docker**-type Web Service.

1. **Push this repository to GitHub** (already at
   `https://github.com/rdmmoonlight/aurum-finance-backend`).

2. **Create a new Web Service on Render**
   - Environment: **Docker**
   - Root directory: repository root (where `Dockerfile` lives)
   - Render automatically builds the image from `Dockerfile` and provides `$PORT`;
     no start command override is needed.

3. **Provision the database on Neon**
   - Create a Neon PostgreSQL project and copy its connection string
     (`postgres://user:password@host/dbname?sslmode=require`).

4. **Set environment variables on the Render service**

   | Key                | Value                                                             |
   |--------------------|--------------------------------------------------------------------|
   | `ASPNETCORE_ENVIRONMENT` | `Production`                                                  |
   | `DATABASE_URL`     | Neon connection string                                             |
   | `ALLOWED_ORIGINS`  | Your frontend origin(s), comma-separated                           |

5. **Deploy.** Render will build the Docker image, start the container, and route
   traffic to it once `/health` reports healthy.

### Notes on the Dockerfile

- Multi-stage build: SDK image compiles and publishes, the smaller ASP.NET runtime image
  runs the published output.
- The container listens on `$PORT` (defaulting to `8080` for local `docker run`), matching
  Render's requirement that the app bind to the port it provides.

## Health checks

`GET /health` reports overall status, including PostgreSQL connectivity, and is suitable
for use as Render's health check path.
