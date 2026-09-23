# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Tech Stack
- .NET 10, ASP.NET Core Minimal APIs
- Entity Framework Core 10 with PostgreSQL
- Mediator for CQRS (source-generated)
- FluentValidation for request validation

## Layout

Two independent apps in one repo, each deployed to its own Azure App Service by a GitHub Actions workflow on push to `main` (`.github/workflows/`):

- `backend/` — ASP.NET Core Web API (`net10.0`, controllers, no minimal APIs). Solution file is `backend/ECommerceApi.slnx`.
- `frontend/` — Express + EJS shell. Pages are empty templates; all data comes from client-side `fetch` calls in `frontend/public/js/*.js` straight to the API. There is no build, lint, or test step.

## Commands

Backend (run from `backend/`):

```bash
dotnet build ECommerceApi.slnx
dotnet run --launch-profile https          # https://localhost:7224 (http on :5002)
dotnet test ECommerceApi.Tests             # whole suite
dotnet test ECommerceApi.Tests --filter "FullyQualifiedName~OrderControllerTests.GetMyOrders_NoOrders_ReturnsNotFound"
dotnet ef migrations add <Name>            # then: dotnet ef database update
```

Frontend (run from `frontend/`): `npm install`, `npm start` (port 3000) or `npm run dev` (nodemon).

## Known state: `main` does not compile

Unresolved merge conflict markers (`<<<<<<< HEAD` … `>>>>>>> dev`) are committed in `Migrations/AppDbContextModelSnapshot.cs` (19 blocks), `Data/Configurations/OrderModelConfiguration.cs`, and `Properties/launchSettings.json`. `dotnet build` fails with CS8300 until they are resolved. In each conflict the `HEAD` side is the old SQLite version and the `dev` side is the SQL Server version that matches the current code (`decimal(18,2)` on `Order.Total`, SQL Server column types in the snapshot). The `dev` branch exists locally and on `origin`. Check `git grep -n "^<<<<<<<"` before assuming a build failure is your own.

## Architecture

**The README is out of date on infrastructure.** It describes SQLite and local secrets; the code now uses:

- **SQL Server** (`UseSqlServer` in `Program.cs`) with Azure AD auth (`Authentication=Active Directory Default` in `appsettings.Development.json`). `Migrations/20260916130455_InitialSqlServer` is the baseline; the earlier migrations were generated against SQLite. `appsettings.Development.json` (holds the connection string and `Jwt` issuer/audience) is gitignored and not in the repo, so a fresh clone needs its own copy.
- **Azure Key Vault** is added as a configuration source in `Program.cs` (`AddAzureKeyVault` with `DefaultAzureCredential`) and is where `Jwt:Key`, `Stripe:SecretKey` and `Redis:Connection` are expected to come from (the checked-in appsettings files leave them empty). Running the API locally requires being signed in to Azure with access to that vault, and a reachable Redis.
- The **tests still use SQLite in-memory** (`TestDb` in `ECommerceApi.Tests/TestInfrastructure.cs`, `EnsureCreated()`), not SQL Server, and construct controllers directly (`new OrderController(db.Context)` plus `TestHelpers.SetUser`) instead of going through `WebApplicationFactory`. So `Program.cs` wiring, rate limiting, and auth middleware are not exercised by tests. `TestHelpers` builds `UserManager`/`SignInManager` by hand for `AuthController` tests.

**Request pipeline** (`Program.cs`): CORS policy `AllowFrontend` (hard-coded localhost:3000 and the Azure frontend origin — new frontend origins must be added there) → rate limiter → auth. `AuthPolicy` is a fixed-window limiter (5 req / 5 min) applied to all of `AuthController` via `[EnableRateLimiting]`. JWT bearer is the only auth scheme; `JwtService` issues tokens with `ClaimTypes.Role` claims, and roles are enforced with `[Authorize(Roles = "Admin")]`. On startup `Program.cs` also seeds the `Admin` and `Customer` roles.

**Data model**: `AppDbContext : IdentityDbContext<User>`. Relationships are configured only via `IEntityTypeConfiguration<T>` classes in `Data/Configurations/` (picked up by `ApplyConfigurationsFromAssembly`), not data annotations. Roles come from Identity's role tables, not a column on `User`.

**Orders / payments** (`OrderController`): `Checkout` converts the cart to an `Order`, decrements `Product.Stock`, creates a Stripe `PaymentIntent` (GBP, amount = total × 100), stores its id on the order as `Pending`, and clears the cart. `OrderItem` snapshots `ProductName` and `UnitPrice` rather than referencing live product data, so don't "fix" it to join against `Product`. Payment is confirmed by polling: `POST /api/order/{id}/confirm-payment` fetches the intent from Stripe and flips the order to `Paid`. There is no webhook. Controllers create `PaymentIntentService` directly, so tests can't exercise the Stripe calls; `Stripe.StripeConfiguration.ApiKey` is set once in `Program.cs`.

**Caching** (`ProductsController`): `GET /api/products` caches the full list under the Redis key `products_all` (5 min TTL) via `IDistributedCache`. Every Admin write (POST/PUT/DELETE) must call `_cache.RemoveAsync("products_all")`; the other read endpoints hit the DB directly.

**Frontend**: `frontend/public/js/config.js` holds `API_BASE` (currently the production Azure URL — switch it to `https://localhost:7224/api` for local work), the `jwt_token` localStorage helpers, and `apiFetch`, which attaches the bearer token and redirects to `/login` on any 401. Admin visibility is decided client-side by decoding the JWT role claim; the server's `[Authorize]` attributes are the real enforcement. API-derived values interpolated into `innerHTML` must go through `escapeHtml`.

## Workflow Rules
- ALWAYS create a git branch before making changes
- Run `dotnet test` after every implementation
- Keep commits atomic and focused