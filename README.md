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
- Migration status: **Authentication**, **Users**, **Accounting** (Accounts +
  Periods), and **Journals** are implemented. **Ledger, Reports, Dashboard**
  are still placeholders, migrated in that order from the NestJS source —
  see each feature's own README.
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

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
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

```
aurum-finance-backend
├─ .dockerignore
├─ ARCHITECTURE.md
├─ Aurum.Api
│  ├─ appsettings.Development.json
│  ├─ appsettings.json
│  ├─ appsettings.Production.json
│  ├─ Aurum.Api.csproj
│  ├─ bin
│  │  └─ Debug
│  │     └─ net10.0
│  │        ├─ appsettings.Development.json
│  │        ├─ appsettings.json
│  │        ├─ appsettings.Production.json
│  │        ├─ Asp.Versioning.Abstractions.dll
│  │        ├─ Asp.Versioning.Http.dll
│  │        ├─ Asp.Versioning.Mvc.ApiExplorer.dll
│  │        ├─ Asp.Versioning.Mvc.dll
│  │        ├─ Aurum.Api.deps.json
│  │        ├─ Aurum.Api.dll
│  │        ├─ Aurum.Api.exe
│  │        ├─ Aurum.Api.pdb
│  │        ├─ Aurum.Api.runtimeconfig.json
│  │        ├─ Aurum.Api.staticwebassets.endpoints.json
│  │        ├─ cs
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ de
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ es
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ FluentValidation.AspNetCore.dll
│  │        ├─ FluentValidation.DependencyInjectionExtensions.dll
│  │        ├─ FluentValidation.dll
│  │        ├─ fr
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ HealthChecks.NpgSql.dll
│  │        ├─ HealthChecks.UI.Client.dll
│  │        ├─ HealthChecks.UI.Core.dll
│  │        ├─ Humanizer.dll
│  │        ├─ it
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ ja
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ ko
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ Microsoft.AspNetCore.Authentication.JwtBearer.dll
│  │        ├─ Microsoft.Bcl.AsyncInterfaces.dll
│  │        ├─ Microsoft.CodeAnalysis.CSharp.dll
│  │        ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.dll
│  │        ├─ Microsoft.CodeAnalysis.dll
│  │        ├─ Microsoft.CodeAnalysis.Workspaces.dll
│  │        ├─ Microsoft.EntityFrameworkCore.Abstractions.dll
│  │        ├─ Microsoft.EntityFrameworkCore.Design.dll
│  │        ├─ Microsoft.EntityFrameworkCore.dll
│  │        ├─ Microsoft.EntityFrameworkCore.Relational.dll
│  │        ├─ Microsoft.Extensions.DependencyModel.dll
│  │        ├─ Microsoft.IdentityModel.Abstractions.dll
│  │        ├─ Microsoft.IdentityModel.JsonWebTokens.dll
│  │        ├─ Microsoft.IdentityModel.Logging.dll
│  │        ├─ Microsoft.IdentityModel.Protocols.dll
│  │        ├─ Microsoft.IdentityModel.Protocols.OpenIdConnect.dll
│  │        ├─ Microsoft.IdentityModel.Tokens.dll
│  │        ├─ Microsoft.OpenApi.dll
│  │        ├─ Mono.TextTemplating.dll
│  │        ├─ Npgsql.dll
│  │        ├─ Npgsql.EntityFrameworkCore.PostgreSQL.dll
│  │        ├─ pl
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ pt-BR
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ ru
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ Serilog.AspNetCore.dll
│  │        ├─ Serilog.dll
│  │        ├─ Serilog.Enrichers.Environment.dll
│  │        ├─ Serilog.Extensions.Hosting.dll
│  │        ├─ Serilog.Extensions.Logging.dll
│  │        ├─ Serilog.Formatting.Compact.dll
│  │        ├─ Serilog.Settings.Configuration.dll
│  │        ├─ Serilog.Sinks.Console.dll
│  │        ├─ Serilog.Sinks.Debug.dll
│  │        ├─ Serilog.Sinks.File.dll
│  │        ├─ Swashbuckle.AspNetCore.Swagger.dll
│  │        ├─ Swashbuckle.AspNetCore.SwaggerGen.dll
│  │        ├─ Swashbuckle.AspNetCore.SwaggerUI.dll
│  │        ├─ System.CodeDom.dll
│  │        ├─ System.Composition.AttributedModel.dll
│  │        ├─ System.Composition.Convention.dll
│  │        ├─ System.Composition.Hosting.dll
│  │        ├─ System.Composition.Runtime.dll
│  │        ├─ System.Composition.TypedParts.dll
│  │        ├─ System.IdentityModel.Tokens.Jwt.dll
│  │        ├─ tr
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ zh-Hans
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        └─ zh-Hant
│  │           ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │           ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │           ├─ Microsoft.CodeAnalysis.resources.dll
│  │           └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  ├─ Common
│  │  └─ PeriodLock
│  │     └─ PeriodLockPolicy.cs
│  ├─ Contracts
│  │  └─ README.md
│  ├─ Core
│  │  ├─ Exceptions
│  │  │  ├─ AppException.cs
│  │  │  ├─ BadRequestException.cs
│  │  │  ├─ ConflictException.cs
│  │  │  ├─ NotFoundException.cs
│  │  │  ├─ UnauthorizedAppException.cs
│  │  │  └─ ValidationAppException.cs
│  │  ├─ Extensions
│  │  │  ├─ AccountingServiceExtensions.cs
│  │  │  ├─ ApiVersioningServiceExtensions.cs
│  │  │  ├─ AuthenticationServiceExtensions.cs
│  │  │  ├─ CorsServiceExtensions.cs
│  │  │  ├─ DatabaseServiceExtensions.cs
│  │  │  ├─ FluentValidationExtensions.cs
│  │  │  ├─ HealthCheckServiceExtensions.cs
│  │  │  ├─ JournalsServiceExtensions.cs
│  │  │  ├─ RateLimitingServiceExtensions.cs
│  │  │  ├─ SecurityServiceExtensions.cs
│  │  │  └─ SwaggerServiceExtensions.cs
│  │  ├─ Middleware
│  │  │  ├─ ExceptionHandlingMiddleware.cs
│  │  │  └─ MiddlewareExtensions.cs
│  │  ├─ Serialization
│  │  │  └─ OptionalJsonConverter.cs
│  │  ├─ Shared
│  │  │  ├─ ApiErrorResponse.cs
│  │  │  ├─ ApiResponse.cs
│  │  │  ├─ ErrorResponse.cs
│  │  │  ├─ OkResult.cs
│  │  │  └─ Optional.cs
│  │  └─ Utilities
│  │     ├─ ConnectionStringHelper.cs
│  │     └─ README.md
│  ├─ Features
│  │  ├─ Accounting
│  │  │  ├─ Accounts
│  │  │  │  ├─ AccountsController.cs
│  │  │  │  ├─ AccountsService.cs
│  │  │  │  ├─ Configurations
│  │  │  │  │  └─ AccountConfiguration.cs
│  │  │  │  ├─ DefaultAccounts.cs
│  │  │  │  ├─ Dtos
│  │  │  │  │  ├─ AccountDto.cs
│  │  │  │  │  ├─ CreateAccountRequest.cs
│  │  │  │  │  ├─ ReorderAccountsRequest.cs
│  │  │  │  │  ├─ ResetAccountsRequest.cs
│  │  │  │  │  └─ UpdateAccountRequest.cs
│  │  │  │  ├─ Entities
│  │  │  │  │  ├─ Account.cs
│  │  │  │  │  └─ AccountRole.cs
│  │  │  │  └─ Validators
│  │  │  │     ├─ CreateAccountRequestValidator.cs
│  │  │  │     ├─ ReorderAccountsRequestValidator.cs
│  │  │  │     └─ UpdateAccountRequestValidator.cs
│  │  │  ├─ Periods
│  │  │  │  ├─ Configurations
│  │  │  │  │  └─ PeriodConfiguration.cs
│  │  │  │  ├─ Dtos
│  │  │  │  │  ├─ CreatePeriodRequest.cs
│  │  │  │  │  ├─ PeriodDto.cs
│  │  │  │  │  └─ UpdatePeriodRequest.cs
│  │  │  │  ├─ Entities
│  │  │  │  │  ├─ Period.cs
│  │  │  │  │  └─ PeriodStatus.cs
│  │  │  │  ├─ PeriodsController.cs
│  │  │  │  ├─ PeriodsService.cs
│  │  │  │  └─ Validators
│  │  │  │     ├─ CreatePeriodRequestValidator.cs
│  │  │  │     └─ UpdatePeriodRequestValidator.cs
│  │  │  └─ README.md
│  │  ├─ Authentication
│  │  │  ├─ AuthController.cs
│  │  │  ├─ AuthService.cs
│  │  │  ├─ Dtos
│  │  │  │  ├─ AuthResponse.cs
│  │  │  │  ├─ LoginRequest.cs
│  │  │  │  └─ RegisterRequest.cs
│  │  │  ├─ README.md
│  │  │  └─ Validators
│  │  │     ├─ LoginRequestValidator.cs
│  │  │     └─ RegisterRequestValidator.cs
│  │  ├─ Dashboard
│  │  │  └─ README.md
│  │  ├─ Journals
│  │  │  ├─ Configurations
│  │  │  │  └─ JournalEntryConfiguration.cs
│  │  │  ├─ Dtos
│  │  │  │  ├─ CreateJournalEntryRequest.cs
│  │  │  │  ├─ JournalEntryDto.cs
│  │  │  │  ├─ JournalEntryRowRequest.cs
│  │  │  │  └─ UpdateJournalEntryRequest.cs
│  │  │  ├─ Entities
│  │  │  │  ├─ JournalEntry.cs
│  │  │  │  └─ JournalKind.cs
│  │  │  ├─ JournalEntriesController.cs
│  │  │  ├─ JournalEntriesService.cs
│  │  │  ├─ README.md
│  │  │  └─ Validators
│  │  │     ├─ CreateJournalEntryRequestValidator.cs
│  │  │     ├─ JournalBalanceRules.cs
│  │  │     └─ UpdateJournalEntryRequestValidator.cs
│  │  ├─ Ledger
│  │  │  └─ README.md
│  │  ├─ Reports
│  │  │  └─ README.md
│  │  └─ Users
│  │     ├─ Configurations
│  │     │  └─ UserConfiguration.cs
│  │     ├─ Entities
│  │     │  └─ User.cs
│  │     └─ README.md
│  ├─ Infrastructure
│  │  ├─ Database
│  │  │  └─ AppDbContext.cs
│  │  ├─ External
│  │  │  └─ README.md
│  │  ├─ Logging
│  │  │  └─ SerilogConfiguration.cs
│  │  └─ Security
│  │     ├─ CurrentUserService.cs
│  │     ├─ ICurrentUserService.cs
│  │     ├─ IJwtTokenService.cs
│  │     ├─ JwtSettings.cs
│  │     ├─ JwtTokenService.cs
│  │     └─ README.md
│  ├─ logs
│  │  └─ aurum-api-20260720.log
│  ├─ obj
│  │  ├─ Aurum.Api.csproj.nuget.dgspec.json
│  │  ├─ Aurum.Api.csproj.nuget.g.props
│  │  ├─ Aurum.Api.csproj.nuget.g.targets
│  │  ├─ Debug
│  │  │  └─ net10.0
│  │  │     ├─ .NETCoreApp,Version=v10.0.AssemblyAttributes.cs
│  │  │     ├─ apphost.exe
│  │  │     ├─ Aurum.Api.AssemblyInfo.cs
│  │  │     ├─ Aurum.Api.AssemblyInfoInputs.cache
│  │  │     ├─ Aurum.Api.assets.cache
│  │  │     ├─ Aurum.Api.csproj.AssemblyReference.cache
│  │  │     ├─ Aurum.Api.csproj.CoreCompileInputs.cache
│  │  │     ├─ Aurum.Api.csproj.FileListAbsolute.txt
│  │  │     ├─ Aurum.Api.csproj.Up2Date
│  │  │     ├─ Aurum.Api.dll
│  │  │     ├─ Aurum.Api.GeneratedMSBuildEditorConfig.editorconfig
│  │  │     ├─ Aurum.Api.genruntimeconfig.cache
│  │  │     ├─ Aurum.Api.GlobalUsings.g.cs
│  │  │     ├─ Aurum.Api.MvcApplicationPartsAssemblyInfo.cache
│  │  │     ├─ Aurum.Api.MvcApplicationPartsAssemblyInfo.cs
│  │  │     ├─ Aurum.Api.pdb
│  │  │     ├─ Aurum.Api.sourcelink.json
│  │  │     ├─ ref
│  │  │     │  └─ Aurum.Api.dll
│  │  │     ├─ refint
│  │  │     │  └─ Aurum.Api.dll
│  │  │     ├─ rjsmcshtml.dswa.cache.json
│  │  │     ├─ rjsmrazor.dswa.cache.json
│  │  │     ├─ rpswa.dswa.cache.json
│  │  │     ├─ staticwebassets
│  │  │     ├─ staticwebassets.build.endpoints.json
│  │  │     ├─ staticwebassets.build.json
│  │  │     ├─ staticwebassets.build.json.cache
│  │  │     └─ swae.build.ex.cache
│  │  ├─ project.assets.json
│  │  └─ project.nuget.cache
│  ├─ Program.cs
│  └─ Properties
│     └─ launchSettings.json
├─ Aurum.Api.sln
├─ COMMAND.txt
├─ Dockerfile
└─ README.md

```
```
aurum-finance-backend
├─ .dockerignore
├─ ARCHITECTURE.md
├─ Aurum.Api
│  ├─ appsettings.Development.json
│  ├─ appsettings.json
│  ├─ appsettings.Production.json
│  ├─ Aurum.Api.csproj
│  ├─ bin
│  │  └─ Debug
│  │     └─ net10.0
│  │        ├─ appsettings.Development.json
│  │        ├─ appsettings.json
│  │        ├─ appsettings.Production.json
│  │        ├─ Asp.Versioning.Abstractions.dll
│  │        ├─ Asp.Versioning.Http.dll
│  │        ├─ Asp.Versioning.Mvc.ApiExplorer.dll
│  │        ├─ Asp.Versioning.Mvc.dll
│  │        ├─ Aurum.Api.deps.json
│  │        ├─ Aurum.Api.dll
│  │        ├─ Aurum.Api.exe
│  │        ├─ Aurum.Api.pdb
│  │        ├─ Aurum.Api.runtimeconfig.json
│  │        ├─ Aurum.Api.staticwebassets.endpoints.json
│  │        ├─ cs
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ de
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ es
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ FluentValidation.AspNetCore.dll
│  │        ├─ FluentValidation.DependencyInjectionExtensions.dll
│  │        ├─ FluentValidation.dll
│  │        ├─ fr
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ HealthChecks.NpgSql.dll
│  │        ├─ HealthChecks.UI.Client.dll
│  │        ├─ HealthChecks.UI.Core.dll
│  │        ├─ Humanizer.dll
│  │        ├─ it
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ ja
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ ko
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ Microsoft.AspNetCore.Authentication.JwtBearer.dll
│  │        ├─ Microsoft.Bcl.AsyncInterfaces.dll
│  │        ├─ Microsoft.CodeAnalysis.CSharp.dll
│  │        ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.dll
│  │        ├─ Microsoft.CodeAnalysis.dll
│  │        ├─ Microsoft.CodeAnalysis.Workspaces.dll
│  │        ├─ Microsoft.EntityFrameworkCore.Abstractions.dll
│  │        ├─ Microsoft.EntityFrameworkCore.Design.dll
│  │        ├─ Microsoft.EntityFrameworkCore.dll
│  │        ├─ Microsoft.EntityFrameworkCore.Relational.dll
│  │        ├─ Microsoft.Extensions.DependencyModel.dll
│  │        ├─ Microsoft.IdentityModel.Abstractions.dll
│  │        ├─ Microsoft.IdentityModel.JsonWebTokens.dll
│  │        ├─ Microsoft.IdentityModel.Logging.dll
│  │        ├─ Microsoft.IdentityModel.Protocols.dll
│  │        ├─ Microsoft.IdentityModel.Protocols.OpenIdConnect.dll
│  │        ├─ Microsoft.IdentityModel.Tokens.dll
│  │        ├─ Microsoft.OpenApi.dll
│  │        ├─ Mono.TextTemplating.dll
│  │        ├─ Npgsql.dll
│  │        ├─ Npgsql.EntityFrameworkCore.PostgreSQL.dll
│  │        ├─ pl
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ pt-BR
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ ru
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ Serilog.AspNetCore.dll
│  │        ├─ Serilog.dll
│  │        ├─ Serilog.Enrichers.Environment.dll
│  │        ├─ Serilog.Extensions.Hosting.dll
│  │        ├─ Serilog.Extensions.Logging.dll
│  │        ├─ Serilog.Formatting.Compact.dll
│  │        ├─ Serilog.Settings.Configuration.dll
│  │        ├─ Serilog.Sinks.Console.dll
│  │        ├─ Serilog.Sinks.Debug.dll
│  │        ├─ Serilog.Sinks.File.dll
│  │        ├─ Swashbuckle.AspNetCore.Swagger.dll
│  │        ├─ Swashbuckle.AspNetCore.SwaggerGen.dll
│  │        ├─ Swashbuckle.AspNetCore.SwaggerUI.dll
│  │        ├─ System.CodeDom.dll
│  │        ├─ System.Composition.AttributedModel.dll
│  │        ├─ System.Composition.Convention.dll
│  │        ├─ System.Composition.Hosting.dll
│  │        ├─ System.Composition.Runtime.dll
│  │        ├─ System.Composition.TypedParts.dll
│  │        ├─ System.IdentityModel.Tokens.Jwt.dll
│  │        ├─ tr
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        ├─ zh-Hans
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │        │  ├─ Microsoft.CodeAnalysis.resources.dll
│  │        │  └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  │        └─ zh-Hant
│  │           ├─ Microsoft.CodeAnalysis.CSharp.resources.dll
│  │           ├─ Microsoft.CodeAnalysis.CSharp.Workspaces.resources.dll
│  │           ├─ Microsoft.CodeAnalysis.resources.dll
│  │           └─ Microsoft.CodeAnalysis.Workspaces.resources.dll
│  ├─ Common
│  │  └─ PeriodLock
│  │     └─ PeriodLockPolicy.cs
│  ├─ Contracts
│  │  └─ README.md
│  ├─ Core
│  │  ├─ Exceptions
│  │  │  ├─ AppException.cs
│  │  │  ├─ BadRequestException.cs
│  │  │  ├─ ConflictException.cs
│  │  │  ├─ NotFoundException.cs
│  │  │  ├─ UnauthorizedAppException.cs
│  │  │  └─ ValidationAppException.cs
│  │  ├─ Extensions
│  │  │  ├─ AccountingServiceExtensions.cs
│  │  │  ├─ ApiVersioningServiceExtensions.cs
│  │  │  ├─ AuthenticationServiceExtensions.cs
│  │  │  ├─ CorsServiceExtensions.cs
│  │  │  ├─ DatabaseServiceExtensions.cs
│  │  │  ├─ FluentValidationExtensions.cs
│  │  │  ├─ HealthCheckServiceExtensions.cs
│  │  │  ├─ JournalsServiceExtensions.cs
│  │  │  ├─ RateLimitingServiceExtensions.cs
│  │  │  ├─ SecurityServiceExtensions.cs
│  │  │  └─ SwaggerServiceExtensions.cs
│  │  ├─ Middleware
│  │  │  ├─ ExceptionHandlingMiddleware.cs
│  │  │  └─ MiddlewareExtensions.cs
│  │  ├─ Serialization
│  │  │  └─ OptionalJsonConverter.cs
│  │  ├─ Shared
│  │  │  ├─ ApiErrorResponse.cs
│  │  │  ├─ ApiResponse.cs
│  │  │  ├─ ErrorResponse.cs
│  │  │  ├─ OkResult.cs
│  │  │  └─ Optional.cs
│  │  └─ Utilities
│  │     ├─ ConnectionStringHelper.cs
│  │     └─ README.md
│  ├─ Features
│  │  ├─ Accounting
│  │  │  ├─ Accounts
│  │  │  │  ├─ AccountsController.cs
│  │  │  │  ├─ AccountsService.cs
│  │  │  │  ├─ Configurations
│  │  │  │  │  └─ AccountConfiguration.cs
│  │  │  │  ├─ DefaultAccounts.cs
│  │  │  │  ├─ Dtos
│  │  │  │  │  ├─ AccountDto.cs
│  │  │  │  │  ├─ CreateAccountRequest.cs
│  │  │  │  │  ├─ ReorderAccountsRequest.cs
│  │  │  │  │  ├─ ResetAccountsRequest.cs
│  │  │  │  │  └─ UpdateAccountRequest.cs
│  │  │  │  ├─ Entities
│  │  │  │  │  ├─ Account.cs
│  │  │  │  │  └─ AccountRole.cs
│  │  │  │  └─ Validators
│  │  │  │     ├─ CreateAccountRequestValidator.cs
│  │  │  │     ├─ ReorderAccountsRequestValidator.cs
│  │  │  │     └─ UpdateAccountRequestValidator.cs
│  │  │  ├─ Periods
│  │  │  │  ├─ Configurations
│  │  │  │  │  └─ PeriodConfiguration.cs
│  │  │  │  ├─ Dtos
│  │  │  │  │  ├─ CreatePeriodRequest.cs
│  │  │  │  │  ├─ PeriodDto.cs
│  │  │  │  │  └─ UpdatePeriodRequest.cs
│  │  │  │  ├─ Entities
│  │  │  │  │  ├─ Period.cs
│  │  │  │  │  └─ PeriodStatus.cs
│  │  │  │  ├─ PeriodsController.cs
│  │  │  │  ├─ PeriodsService.cs
│  │  │  │  └─ Validators
│  │  │  │     ├─ CreatePeriodRequestValidator.cs
│  │  │  │     └─ UpdatePeriodRequestValidator.cs
│  │  │  └─ README.md
│  │  ├─ Authentication
│  │  │  ├─ AuthController.cs
│  │  │  ├─ AuthService.cs
│  │  │  ├─ Dtos
│  │  │  │  ├─ AuthResponse.cs
│  │  │  │  ├─ LoginRequest.cs
│  │  │  │  └─ RegisterRequest.cs
│  │  │  ├─ README.md
│  │  │  └─ Validators
│  │  │     ├─ LoginRequestValidator.cs
│  │  │     └─ RegisterRequestValidator.cs
│  │  ├─ Dashboard
│  │  │  └─ README.md
│  │  ├─ Journals
│  │  │  ├─ Configurations
│  │  │  │  └─ JournalEntryConfiguration.cs
│  │  │  ├─ Dtos
│  │  │  │  ├─ CreateJournalEntryRequest.cs
│  │  │  │  ├─ JournalEntryDto.cs
│  │  │  │  ├─ JournalEntryRowRequest.cs
│  │  │  │  └─ UpdateJournalEntryRequest.cs
│  │  │  ├─ Entities
│  │  │  │  ├─ JournalEntry.cs
│  │  │  │  └─ JournalKind.cs
│  │  │  ├─ JournalEntriesController.cs
│  │  │  ├─ JournalEntriesService.cs
│  │  │  ├─ README.md
│  │  │  └─ Validators
│  │  │     ├─ CreateJournalEntryRequestValidator.cs
│  │  │     ├─ JournalBalanceRules.cs
│  │  │     └─ UpdateJournalEntryRequestValidator.cs
│  │  ├─ Ledger
│  │  │  └─ README.md
│  │  ├─ Reports
│  │  │  └─ README.md
│  │  └─ Users
│  │     ├─ Configurations
│  │     │  └─ UserConfiguration.cs
│  │     ├─ Entities
│  │     │  └─ User.cs
│  │     └─ README.md
│  ├─ Infrastructure
│  │  ├─ Database
│  │  │  └─ AppDbContext.cs
│  │  ├─ External
│  │  │  └─ README.md
│  │  ├─ Logging
│  │  │  └─ SerilogConfiguration.cs
│  │  └─ Security
│  │     ├─ CurrentUserService.cs
│  │     ├─ ICurrentUserService.cs
│  │     ├─ IJwtTokenService.cs
│  │     ├─ JwtSettings.cs
│  │     ├─ JwtTokenService.cs
│  │     └─ README.md
│  ├─ logs
│  │  └─ aurum-api-20260720.log
│  ├─ obj
│  │  ├─ Aurum.Api.csproj.nuget.dgspec.json
│  │  ├─ Aurum.Api.csproj.nuget.g.props
│  │  ├─ Aurum.Api.csproj.nuget.g.targets
│  │  ├─ Debug
│  │  │  └─ net10.0
│  │  │     ├─ .NETCoreApp,Version=v10.0.AssemblyAttributes.cs
│  │  │     ├─ apphost.exe
│  │  │     ├─ Aurum.Api.AssemblyInfo.cs
│  │  │     ├─ Aurum.Api.AssemblyInfoInputs.cache
│  │  │     ├─ Aurum.Api.assets.cache
│  │  │     ├─ Aurum.Api.csproj.AssemblyReference.cache
│  │  │     ├─ Aurum.Api.csproj.CoreCompileInputs.cache
│  │  │     ├─ Aurum.Api.csproj.FileListAbsolute.txt
│  │  │     ├─ Aurum.Api.csproj.Up2Date
│  │  │     ├─ Aurum.Api.dll
│  │  │     ├─ Aurum.Api.GeneratedMSBuildEditorConfig.editorconfig
│  │  │     ├─ Aurum.Api.genruntimeconfig.cache
│  │  │     ├─ Aurum.Api.GlobalUsings.g.cs
│  │  │     ├─ Aurum.Api.MvcApplicationPartsAssemblyInfo.cache
│  │  │     ├─ Aurum.Api.MvcApplicationPartsAssemblyInfo.cs
│  │  │     ├─ Aurum.Api.pdb
│  │  │     ├─ Aurum.Api.sourcelink.json
│  │  │     ├─ ref
│  │  │     │  └─ Aurum.Api.dll
│  │  │     ├─ refint
│  │  │     │  └─ Aurum.Api.dll
│  │  │     ├─ rjsmcshtml.dswa.cache.json
│  │  │     ├─ rjsmrazor.dswa.cache.json
│  │  │     ├─ rpswa.dswa.cache.json
│  │  │     ├─ staticwebassets
│  │  │     ├─ staticwebassets.build.endpoints.json
│  │  │     ├─ staticwebassets.build.json
│  │  │     ├─ staticwebassets.build.json.cache
│  │  │     └─ swae.build.ex.cache
│  │  ├─ project.assets.json
│  │  └─ project.nuget.cache
│  ├─ Program.cs
│  └─ Properties
│     └─ launchSettings.json
├─ Aurum.Api.sln
├─ COMMAND.txt
├─ Dockerfile
└─ README.md

```