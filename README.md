# Aurum API

Backend for the Aurum personal finance application, built with ASP.NET
Core (C#) using a modular, feature-based architecture inspired by Clean
Architecture and Vertical Slice Architecture.

Today this API implements one feature end to end: **Authentication**
(register, login, refresh tokens, password reset, email verification,
role-based authorization). Everything else — accounting, journals,
ledger, reports, bank sync — is intentionally not here yet; add each as
its own `Features/<Name>` folder when it's actually needed, built fresh
against this codebase rather than ported from anywhere else.

## Architecture

```
Aurum.Api
├── Features/
│   ├── Authentication/    Register, login, refresh, password reset,
│   │                      email verification — see its own README
│   └── Users/              The User entity Authentication operates on
├── Core/                   Cross-cutting application concerns
│   ├── Middleware/          Global exception handling
│   ├── Extensions/          DI wiring (Swagger, CORS, rate limiting,
│   │                        versioning, DB, health, auth)
│   ├── Exceptions/          AppException hierarchy → consistent HTTP error responses
│   ├── Shared/               ErrorResponse — the {error} shape every response uses
│   └── Utilities/            Stateless helpers (e.g. connection string parsing)
├── Infrastructure/         Technical concerns, isolated from business logic
│   ├── Database/            EF Core AppDbContext (PostgreSQL via Npgsql)
│   ├── Email/                IEmailSender abstraction (logs by default — see
│   │                          Authentication's README)
│   ├── Logging/               Serilog configuration
│   └── Security/               JWT issuing, refresh-token rotation, current-user
│                                resolution
├── Migrations/             EF Core migrations for the schema above
├── Program.cs
└── appsettings.json
```

**Rules this project follows:**
- Each feature is self-contained; business logic never lives in `Infrastructure/`.
- `AppDbContext` is used directly — no repository abstraction until one earns its keep.
- The structure is CQRS-ready (a feature can add its own `Commands/`/`Queries/` folders),
  but MediatR is intentionally **not** included yet — add it only when a feature actually
  needs command/query separation.
- Every response — success or error — is returned **flat, with no wrapper**: a bare
  object on success, `{ "error": "message" }` on failure. See
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

2. **Configure secrets**

   Copy `.env.example` to `.env` and fill in `DATABASE_URL` and
   `JWT_SIGNING_KEY` (see the file for details), or set the same values via
   `dotnet user-secrets` / real environment variables. Neither has a
   default — the app throws on startup if either is missing.

3. **Create the schema**

   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

   (Run `dotnet tool install --global dotnet-ef` first if you don't have
   the EF Core CLI tool yet.)

4. **Run the API**

   ```bash
   dotnet run
   ```

   The API starts at `http://localhost:5080` (or `$PORT` if set) with
   Swagger UI at `/swagger`. Health checks are available at `/health`.

## Configuration reference

| Setting                         | Environment variable            | Purpose                                   |
|----------------------------------|----------------------------------|--------------------------------------------|
| `ConnectionStrings:DefaultConnection` | `DATABASE_URL`              | PostgreSQL connection (URL or keyword=value) |
| `Jwt:SigningKey`                 | `JWT_SIGNING_KEY`                 | JWT signing secret — required, no default |
| `Cors:AllowedOrigins`            | `ALLOWED_ORIGINS` (comma-separated) | Origins allowed to call the API        |
| `RateLimiting:PermitLimit`       | `RATELIMITING__PERMITLIMIT`      | Requests allowed per window, per client IP |
| `RateLimiting:WindowSeconds`     | `RATELIMITING__WINDOWSECONDS`    | Length of the rate limit window in seconds |
| —                                | `PORT`                           | Port Kestrel binds to inside the container |

Environment variables always take priority over `appsettings*.json` values, which is the
convention used for Render deployment. See `.env.example` for the full list, including
Authentication's `Auth:*` lockout/token-expiry settings.

## Deploying to Render.com

This repository includes a production `Dockerfile` at the repository root, so Render can
build and run the API as a **Docker**-type Web Service.

1. **Push this repository to GitHub.**

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
   | `JWT_SIGNING_KEY`  | A long random secret — never reuse a development value             |
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
