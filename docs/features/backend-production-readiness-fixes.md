## Summary

Validation performed:

- `dotnet build server\FinancesControl.sln` passes, with warnings.

- `dotnet test server\FinancesControl.sln` passes: 307 passed, 0 failed, with warnings.

## Security

- [x] **Scope transaction category ownership in transaction validation**

  - **Where:** `TransactionService.ValidateForeignKeys`, `TransactionService.Update`, `TransactionCategoryService.ExistsAsync`, `TransactionCategoryRepository.ExistsAsync`, `TransactionCategoryExistsQuery`

  - **Impact:** Transactions can reference another user's custom category by ID because category existence is checked globally instead of by current user/default category visibility.

  - **Hint:** Change category existence checks to accept `userId` and return true only for default categories or categories owned by that user. Update tests for cross-user category rejection.

- [x] **Harden Identity options for production**

  - **Where:** `Program.cs`, `AddIdentityApiEndpoints<IdentityUser>()`

  - **Impact:** Password, lockout, sign-in, and user options rely on framework defaults, which may not match the security posture expected for a production finance app.

  - **Hint:** Configure explicit `IdentityOptions`: password length/complexity, unique email, lockout thresholds, and confirmed-account policy if email confirmation will be used.

- [ ] **Review and restrict public authentication endpoints**

  - **Where:** `Program.cs`, `app.MapIdentityApi<IdentityUser>()`

  - **Impact:** The app exposes default Identity API endpoints. Production may unintentionally allow public registration or unsupported auth flows.

  - **Hint:** Decide which Identity endpoints are allowed in production. If registration must be controlled, replace or wrap default endpoints with explicit auth controllers/policies.

- [ ] **Add API rate limiting**

  - **Where:** `Program.cs`

  - **Impact:** Login, registration, and finance write endpoints have no app-level request throttling, increasing brute-force and abuse risk.

  - **Hint:** Use ASP.NET Core rate limiting policies. Apply stricter limits to auth endpoints and reasonable per-user/per-IP limits to authenticated API routes.

## Configuration

- [x] **Add production configuration requirements**

  - **Where:** `appsettings.json`, deployment configuration, `Program.cs`

  - **Impact:** `DefaultConnection` and `Cors:AllowedOrigins` are not defined in committed config, and `AllowedHosts` is `"*"`. Missing or permissive production config can cause startup failure or broad host/CORS exposure.

  - **Hint:** Keep secrets outside source control, document required environment variables, fail fast when required config is missing, and set production `AllowedHosts`/CORS origins explicitly.

  - **Required production configuration:** set `ASPNETCORE_ENVIRONMENT=Production`, `ConnectionStrings__DefaultConnection`, `Cors__AllowedOrigins__0` (and `Cors__AllowedOrigins__1`, etc. for additional allowed frontend origins), and `AllowedHosts`.

  - **Secret handling:** provide connection strings through environment variables, deployment secrets, or a secret store. Do not commit real production credentials in `appsettings.json`.

  - **Host/CORS format:** `AllowedHosts` must use concrete host names without scheme or port, such as `api.example.com` or semicolon-delimited values. CORS origins must include scheme plus host and optional port, such as `https://app.example.com`, and must not end with `/`.

## Database

- [ ] **Fix database migration scripts for production safety and conventions**

  - **Where:** `Database/Tables/1.0.0/v1-0-0_create_credit_card_details_table.sql`, `Database/Tables/1.0.1/v1-0-1_add_accountid_column_cards.sql`

  - **Impact:** `adding `Cards.AccountId BIGINT NOT NULL` without default/backfill can fail on databases with existing rows.

  - **Hint:** Split the card account migration into nullable add, deterministic backfill, constraint validation, then `SET NOT NULL`.

## Code Quality And Reliability

- [ ] **Suppress HTTPS redirection noise in backend tests**

 - **Where:** `Program.cs`, backend integration test host setup using `WebApplicationFactory`

 - **Impact:** Backend tests log repeated `HttpsRedirectionMiddleware` warnings because the test host does not expose a real HTTPS endpoint or configured HTTPS port. The warnings do not fail tests, but they make test output noisy and can hide meaningful warnings.

 - **Hint:** Configure tests to run under a dedicated environment such as `Testing`, and skip `app.UseHttpsRedirection()` for that environment. Alternatively, configure `HttpsRedirection:HttpsPort` in test configuration if redirect behavior must stay active in tests. Leave EF Core InMemory save logs alone unless broader test logging cleanup is planned.

- [ ] **Limit pagination page size**

  - **Where:** `Common/PaginationParams.cs`; list endpoints for accounts, banks, cards, categories, and transactions.

  - **Impact:** Any authenticated user can request an arbitrarily large `PageSize`, which can cause high memory/CPU/database load in production.

  - **Hint:** Add a documented maximum, for example `MaxPageSize = 100`, and clamp positive values above the max. Add tests for negative, zero, default, and oversized page sizes.

- [ ] **Clean build/test warnings before release**

  - **Where:** API response DTOs, transaction DTOs, tests under `server/FinancesControl.Tests`

  - **Impact:** The backend builds and tests pass, but many nullable and unawaited-call warnings can hide real defects and weaken CI signal.

  - **Hint:** Make nullable response payloads explicit, initialize DTO properties or convert to records, and await all async substitute/test calls. Consider treating warnings as errors in CI after cleanup.

## Assumptions

- Banks are intentionally user-based, not account-based.

- The Markdown contains only actionable production-readiness tasks, not a full audit narrative.

- Existing docs should not be modified.
