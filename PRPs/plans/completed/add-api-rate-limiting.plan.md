# Feature: Add API Rate Limiting

## Summary

Add ASP.NET Core rate limiting to protect public Identity endpoints and authenticated finance write routes from brute-force and abuse traffic. The implementation should introduce common policy names/configuration in the API, wire `AddRateLimiter` and `UseRateLimiter` into `Program.cs`, apply a strict auth policy to Identity minimal APIs, apply a finance-write policy to POST/PUT/PATCH/DELETE controller actions, and return the existing JSON error response shape for `429 Too Many Requests`.

## User Story

As an authenticated finance app user
I want abusive request bursts to be throttled before they hit authentication and write operations
So that my financial workspace remains stable and harder to attack

## Problem Statement

Login, registration, and finance write endpoints currently have no app-level request throttling. A client can repeatedly hit Identity endpoints or write routes until downstream authentication, service, repository, and database code absorb the load.

## Solution Statement

Use built-in ASP.NET Core rate limiting for .NET 8. Create a small common configuration class with named policies and partition helpers, configure `builder.Services.AddRateLimiter(...)` in `Program.cs`, call `app.UseRateLimiter()` after authentication so user-based partitions can be resolved, attach the auth policy to `app.MapIdentityApi<IdentityUser>()`, and attach the finance-write policy with `[EnableRateLimiting]` attributes on mutating controller actions.

## Metadata

| Field            | Value |
| ---------------- | ----- |
| Type             | ENHANCEMENT |
| Complexity       | MEDIUM |
| Systems Affected | Backend API startup, ASP.NET Core Identity endpoints, finance controllers, common tests |
| Dependencies     | .NET 8 shared framework, Microsoft.AspNetCore.RateLimiting, System.Threading.RateLimiting, Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.23 |
| Estimated Tasks  | 6 |

---

## Lifecycle (append-only)

- **Created:** 2026-07-16T18:38:39.4218398-03:00
- **Modified:**
  - 2026-07-16T18:38:39.4218398-03:00 - Initial plan created from `PRPs/tasks/add-api-rate-limiting.md`.
  - 2026-07-16T18:55:56.1212393-03:00 - Implemented API rate limiting and validation tests.
- **Commits:** None yet.
- **Agent / Session:**
  - Codex GPT-5 planning session, 2026-07-16.
  - Codex GPT-5 implementation session, 2026-07-16.
- **Back refs:**
  - `PRPs/tasks/add-api-rate-limiting.md` - source task.
  - `docs/features/backend-production-readiness-fixes.md` - production readiness theme.
- **Forward refs:** None yet.

> **Append-only:** `Created` is set once; every other field is a list you only ever add to - never overwrite or remove existing entries. Keep references bidirectional: when you add a back/forward ref here, add the reciprocal ref on the other plan.

---

## UX Design

### Before State

```
+------------------------------------------------------------------------------+
|                              BEFORE STATE                                     |
+------------------------------------------------------------------------------+
|                                                                              |
|   +-------------+         +-------------+         +-------------+             |
|   | API Client  | ------> | Auth/Write  | ------> | Identity or |             |
|   |             |         | Endpoint    |         | Finance Svc |             |
|   +-------------+         +-------------+         +-------------+             |
|                                                                              |
|   USER_FLOW: Clients can repeatedly call /login, /register, and mutating     |
|   finance endpoints without an application-level throttle.                   |
|   PAIN_POINT: Brute-force and burst write traffic reaches auth/service/data  |
|   code before any app-level rejection.                                       |
|   DATA_FLOW: request -> exception middleware -> CORS/auth/authz -> endpoint  |
|   -> service/repository/database.                                            |
|                                                                              |
+------------------------------------------------------------------------------+
```

### After State

```
+------------------------------------------------------------------------------+
|                               AFTER STATE                                     |
+------------------------------------------------------------------------------+
|                                                                              |
|   +-------------+         +-------------+         +-------------+             |
|   | API Client  | ------> | Rate Limit  | ------> | Auth/Write  |             |
|   |             |         | Policy      |         | Endpoint    |             |
|   +-------------+         +-------------+         +-------------+             |
|                                  |                                           |
|                                  v                                           |
|                          +-------------+                                     |
|                          | 429 JSON    |  <- when policy budget is spent     |
|                          +-------------+                                     |
|                                                                              |
|   USER_FLOW: Normal requests pass through unchanged; excessive bursts get    |
|   a 429 response before auth/write work continues.                           |
|   VALUE_ADD: Brute-force login/register attempts and finance write bursts    |
|   are limited per IP/user partition.                                         |
|   DATA_FLOW: request -> exception middleware -> CORS/authentication ->       |
|   rate limiter -> authorization -> endpoint -> service/repository/database.  |
|                                                                              |
+------------------------------------------------------------------------------+
```

### Interaction Changes

| Location | Before | After | User Impact |
|----------|--------|-------|-------------|
| `/login`, `/register` from `app.MapIdentityApi<IdentityUser>()` | Unlimited app-level attempts | Strict fixed-window auth policy by IP | Attack bursts receive `429`; normal users see no change |
| Finance write actions | Unlimited app-level write bursts | Named finance-write policy by authenticated user, falling back to IP | One user cannot flood write endpoints indefinitely |
| Rejection responses | No `429` path | JSON `ErrorResponse` with `httpStatusCode` and `errors` | API clients can handle throttling consistently |

---

## Mandatory Reading

**CRITICAL: Implementation agent MUST read these files before starting any task:**

| Priority | File | Lines | Why Read This |
|----------|------|-------|---------------|
| P0 | `server/FinancesControl.API/Program.cs` | 18-109 | Service registration, Identity mapping, middleware order, and controller mapping live here |
| P0 | `server/FinancesControl.API/Common/Middleware/GlobalExceptionHandlerMiddleware.cs` | 30-55 | Mirror JSON error response shape for rate-limit rejection |
| P0 | `server/FinancesControl.API/Common/Responses/BaseResponse.cs` | 3-20 | Common response contract for `errors` and `httpStatusCode` |
| P1 | `server/FinancesControl.API/Modules/Auth/Controllers/AuthController.cs` | 7-22 | Local logout endpoint is root-routed and authorized |
| P1 | `server/FinancesControl.API/Modules/Accounts/Controllers/AccountController.cs` | 22-92 | Account write endpoints to annotate |
| P1 | `server/FinancesControl.API/Modules/Banks/Controllers/BankController.cs` | 28-77 | Bank write endpoints to annotate |
| P1 | `server/FinancesControl.API/Modules/Cards/Controllers/CardController.cs` | 30-107 | Card write endpoints to annotate |
| P1 | `server/FinancesControl.API/Modules/TransactionCategories/Controllers/TransactionCategoryController.cs` | 27-75 | Category write endpoints to annotate |
| P1 | `server/FinancesControl.API/Modules/Transactions/Controllers/TransactionController.cs` | 27-66 | Transaction write endpoints to annotate |
| P2 | `server/FinancesControl.Tests/Common/GlobalExceptionHandlerMiddlewareTests.cs` | 11-79 | Mirror middleware response test style |
| P2 | `server/FinancesControl.Tests/Common/IdentityOptionsConfigurationTests.cs` | 9-58 | Mirror common static configuration test style |

**External Documentation:**
| Source | Section | Why Needed |
|--------|---------|------------|
| [ASP.NET Core rate limiting middleware for .NET 8+](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0#use-rate-limiting-middleware) | Use Rate Limiting Middleware | Shows `AddRateLimiter`, named policies, `UseRateLimiter`, and endpoint attachment |
| [ASP.NET Core rate limiting middleware for .NET 8+](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0#rate-limiting-partitions) | Rate Limiting Partitions | Shows partitioning by IP/user so limits are fair per caller |
| [ASP.NET Core rate limiting middleware for .NET 8+](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0#fixed-window-limiter) | Fixed window limiter | Supports simple predictable request budgets and notes `QueueLimit = 0` disables queueing |

---

## Patterns to Mirror

**NAMING_CONVENTION:**

```csharp
// SOURCE: server/FinancesControl.API/Common/IdentityOptionsConfiguration.cs:5-8
// COPY THIS PATTERN:
public static class IdentityOptionsConfiguration
{
    public static void Configure(IdentityOptions options)
```

**ERROR_HANDLING:**

```csharp
// SOURCE: server/FinancesControl.API/Common/Middleware/GlobalExceptionHandlerMiddleware.cs:46-54
// COPY THIS PATTERN:
var response = new ErrorResponse();
response.SetHttpStatusCode(statusCode);
response.AddError(message);

context.Response.Clear();
context.Response.StatusCode = statusCode;
context.Response.ContentType = "application/json";
await context.Response.WriteAsJsonAsync(response);
```

**LOGGING_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Common/Middleware/GlobalExceptionHandlerMiddleware.cs:41-44
// COPY THIS PATTERN:
if (shouldLogAsError)
    _logger.LogError(exception, "Unhandled exception while processing the request.");
else
    _logger.LogInformation(exception, "Handled application exception while processing the request.");
```

**REPOSITORY_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Program.cs:58-59
// COPY THIS PATTERN:
// Database connection factory for Dapper
builder.Services.AddScoped<AppDbConnectionFactory>();
```

No repository or database code is needed for this feature. Keep rate limiting in API startup/common infrastructure.

**SERVICE_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Program.cs:61-64
// COPY THIS PATTERN:
builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<IdentityUser>(IdentityOptionsConfiguration.Configure)
    .AddEntityFrameworkStores<FinancesDbContext>();
```

**TEST_STRUCTURE:**

```csharp
// SOURCE: server/FinancesControl.Tests/Common/IdentityOptionsConfigurationTests.cs:9-16
// COPY THIS PATTERN:
[Fact]
public void Configure_WithDefaultOptions_SetsProductionPasswordOptions()
{
    var options = new IdentityOptions();

    IdentityOptionsConfiguration.Configure(options);

    options.Password.RequiredLength.Should().Be(12);
```

---

## Files to Change

| File | Action | Justification |
| ---- | ------ | ------------- |
| `server/FinancesControl.API/Common/RateLimiting/RateLimitPolicyNames.cs` | CREATE | Centralize policy names used by `Program.cs`, attributes, and tests |
| `server/FinancesControl.API/Common/RateLimiting/ApiRateLimitOptions.cs` | CREATE | Keep default limits readable and testable without scattering magic numbers |
| `server/FinancesControl.API/Common/RateLimiting/ApiRateLimitingConfiguration.cs` | CREATE | Encapsulate `RateLimiterOptions` setup, partition keys, and rejection response |
| `server/FinancesControl.API/Program.cs` | UPDATE | Register rate limiting, enable middleware, and attach auth policy to Identity endpoints |
| `server/FinancesControl.API/Modules/Accounts/Controllers/AccountController.cs` | UPDATE | Apply finance-write policy to mutating account actions |
| `server/FinancesControl.API/Modules/Banks/Controllers/BankController.cs` | UPDATE | Apply finance-write policy to mutating bank actions |
| `server/FinancesControl.API/Modules/Cards/Controllers/CardController.cs` | UPDATE | Apply finance-write policy to mutating card actions |
| `server/FinancesControl.API/Modules/TransactionCategories/Controllers/TransactionCategoryController.cs` | UPDATE | Apply finance-write policy to mutating category actions |
| `server/FinancesControl.API/Modules/Transactions/Controllers/TransactionController.cs` | UPDATE | Apply finance-write policy to mutating transaction actions |
| `server/FinancesControl.Tests/Common/ApiRateLimitingConfigurationTests.cs` | CREATE | Verify policy names, partition behavior, and `429` JSON response |

---

## NOT Building (Scope Limits)

Explicit exclusions to prevent scope creep:

- No distributed rate limiter or Redis backing store. This plan covers single-process ASP.NET Core built-in rate limiting only.
- No database schema changes or migrations. Rate-limit counters are in-memory middleware state.
- No frontend UI changes. Clients will receive standard HTTP `429` JSON responses.
- No replacement of ASP.NET Core Identity endpoints. Existing `MapIdentityApi<IdentityUser>()` remains the auth endpoint source.
- No broad global limiter for all read endpoints unless explicitly added later. This task targets auth endpoints and finance writes.

---

## Step-by-Step Tasks

Execute in order. Each task is atomic and independently verifiable.

**Status markers** - prefix EVERY task header with one; the build agent updates it inline as it works: `[ ]` idle . `[wip]` in progress . `[x]` complete . `[f]` failed. All tasks start `[ ]`. If a task cannot be made to pass, mark it `[f]`, record why in Agent Notes, and move on if the rest of the plan can still proceed.

### `[x]` Task 1: CREATE `server/FinancesControl.API/Common/RateLimiting/RateLimitPolicyNames.cs`

- **ACTION**: CREATE constants for named policies.
- **IMPLEMENT**: Add `public static class RateLimitPolicyNames` with `public const string Auth = "auth";` and `public const string FinanceWrite = "finance-write";`.
- **MIRROR**: `server/FinancesControl.API/Common/IdentityOptionsConfiguration.cs:5-8` - static common infrastructure class naming.
- **IMPORTS**: None.
- **GOTCHA**: Named policies must match every `[EnableRateLimiting]` and `RequireRateLimiting` call exactly.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 2: CREATE `server/FinancesControl.API/Common/RateLimiting/ApiRateLimitOptions.cs`

- **ACTION**: CREATE rate-limit defaults.
- **IMPLEMENT**: Add `public sealed class ApiRateLimitOptions` with public init properties for auth and finance limits. Recommended defaults: auth `PermitLimit = 5`, `Window = TimeSpan.FromMinutes(1)`, `QueueLimit = 0`; finance write `PermitLimit = 60`, `Window = TimeSpan.FromMinutes(1)`, `QueueLimit = 0`.
- **MIRROR**: `server/FinancesControl.API/Common/IdentityOptionsConfiguration.cs:7-18` - production hardening defaults kept in common infrastructure.
- **IMPORTS**: None unless TimeSpan requires `using System;` with local style.
- **GOTCHA**: Queueing throttled login attempts can hold connections open; Microsoft docs note `QueueLimit = 0` disables queuing.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 3: CREATE `server/FinancesControl.API/Common/RateLimiting/ApiRateLimitingConfiguration.cs`

- **ACTION**: CREATE centralized rate-limiter configuration.
- **IMPLEMENT**: Add a static `Configure(RateLimiterOptions options, ApiRateLimitOptions limits)` method. Configure:
  - `options.RejectionStatusCode = StatusCodes.Status429TooManyRequests`.
  - `options.OnRejected` to set `Retry-After` when available, set status `429`, content type `application/json`, and write an `ErrorResponse` with `HttpStatusCode = 429` and a non-sensitive message such as `Too many requests. Please try again later.`.
  - `Auth` fixed-window policy partitioned by `HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"`.
  - `FinanceWrite` fixed-window policy partitioned by `ClaimTypes.NameIdentifier`, falling back to remote IP for missing identity.
- **MIRROR**: `server/FinancesControl.API/Common/Middleware/GlobalExceptionHandlerMiddleware.cs:46-54` - common JSON error response shape.
- **IMPORTS**: `using System.Globalization;`, `using System.Security.Claims;`, `using System.Threading.RateLimiting;`, `using FinancesControl.API.Common.Responses;`, `using Microsoft.AspNetCore.RateLimiting;`.
- **GOTCHA**: User-based partitioning only works after authentication has populated `HttpContext.User`; do not place `UseRateLimiter` before `UseAuthentication` for finance-write policy behavior.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 4: UPDATE `server/FinancesControl.API/Program.cs`

- **ACTION**: UPDATE service registration, Identity mapping, and middleware pipeline.
- **IMPLEMENT**:
  - Add `using FinancesControl.API.Common.RateLimiting;`.
  - Register rate limiting after Identity/authorization service setup with `builder.Services.AddRateLimiter(options => ApiRateLimitingConfiguration.Configure(options, new ApiRateLimitOptions()));`.
  - Change `app.MapIdentityApi<IdentityUser>();` to `app.MapIdentityApi<IdentityUser>().RequireRateLimiting(RateLimitPolicyNames.Auth);`.
  - Add `app.UseRateLimiter();` after `app.UseAuthentication();` and before `app.UseAuthorization();`.
- **MIRROR**: `server/FinancesControl.API/Program.cs:61-64` - common auth-related services are registered centrally in startup.
- **IMPORTS**: `using FinancesControl.API.Common.RateLimiting;`.
- **GOTCHA**: Microsoft docs state `UseRateLimiter` must be called in the pipeline to enable policies; endpoint-specific metadata requires middleware ordering where endpoint information is available, and finance partitions require authentication first.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 5: UPDATE finance controllers with `FinanceWrite` policy attributes

- **ACTION**: UPDATE `AccountController.cs`, `BankController.cs`, `CardController.cs`, `TransactionCategoryController.cs`, and `TransactionController.cs`.
- **IMPLEMENT**: Add `using FinancesControl.API.Common.RateLimiting;` and `using Microsoft.AspNetCore.RateLimiting;` where needed. Add `[EnableRateLimiting(RateLimitPolicyNames.FinanceWrite)]` only to mutating finance actions:
  - Accounts: `CreateAccount`, `Update`, `Activate`, `Deactivate`.
  - Banks: `CreateBank`, `Update`, `Activate`, `Deactivate`.
  - Cards: `CreateDebitCard`, `CreateCreditCard`, `UpdateCardName`, `UpdateCreditCardDates`, `Activate`, `Deactivate`.
  - Transaction categories: `Create`, `Rename`, `Activate`, `Deactivate`.
  - Transactions: `Create`, `Delete`, `Update`.
- **MIRROR**: `server/FinancesControl.API/Modules/Cards/Controllers/CardController.cs:30-100` - existing action-level HTTP verb attributes sit directly above each action.
- **IMPORTS**: `using FinancesControl.API.Common.RateLimiting;` and `using Microsoft.AspNetCore.RateLimiting;`.
- **GOTCHA**: Do not rate-limit GET/read actions under this task; scope is finance write endpoints.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 6: CREATE `server/FinancesControl.Tests/Common/ApiRateLimitingConfigurationTests.cs`

- **ACTION**: CREATE tests for common rate-limiting infrastructure.
- **IMPLEMENT**: Add xUnit/FluentAssertions tests that:
  - Call `ApiRateLimitingConfiguration.Configure(new RateLimiterOptions(), new ApiRateLimitOptions())` and verify policies can lease under `RateLimitPolicyNames.Auth` and `RateLimitPolicyNames.FinanceWrite`.
  - Verify auth partitions differ for different remote IPs.
  - Verify finance partitions use `ClaimTypes.NameIdentifier` when authenticated and remote IP fallback when missing.
  - Invoke `OnRejected` with a `DefaultHttpContext` and rejected lease metadata where practical, then assert `StatusCode == 429`, `ContentType == "application/json"`, `httpStatusCode == 429`, and the first `errors` entry contains the safe throttle message.
- **MIRROR**: `server/FinancesControl.Tests/Common/GlobalExceptionHandlerMiddlewareTests.cs:50-78` - response body memory stream and JSON assertion style.
- **IMPORTS**: `using System.Security.Claims;`, `using System.Text.Json;`, `using System.Threading.RateLimiting;`, `using FinancesControl.API.Common.RateLimiting;`, `using FluentAssertions;`, `using Microsoft.AspNetCore.Http;`, `using Microsoft.AspNetCore.RateLimiting;`.
- **GOTCHA**: Avoid full end-to-end Identity integration in this task; current tests favor direct common configuration tests and controller unit tests.
- **VALIDATE**: `dotnet test server/FinancesControl.sln`

---

## Testing Strategy

### Unit Tests to Write

| Test File | Test Cases | Validates |
| --------- | ---------- | --------- |
| `server/FinancesControl.Tests/Common/ApiRateLimitingConfigurationTests.cs` | Policy registration and successful lease below limit | Named policies are configured |
| `server/FinancesControl.Tests/Common/ApiRateLimitingConfigurationTests.cs` | Different IPs get independent auth partitions | Public auth throttling is per caller |
| `server/FinancesControl.Tests/Common/ApiRateLimitingConfigurationTests.cs` | Authenticated users get independent finance partitions | Finance writes are per user |
| `server/FinancesControl.Tests/Common/ApiRateLimitingConfigurationTests.cs` | Rejection callback emits JSON `429` | API clients receive consistent error shape |

### Edge Cases Checklist

- [ ] Remote IP is null and partition falls back to `"unknown"` without throwing.
- [ ] User is authenticated but missing `ClaimTypes.NameIdentifier`; finance policy falls back to IP.
- [ ] Excess requests reject immediately instead of queueing.
- [ ] `Retry-After` is emitted when lease metadata provides it.
- [ ] Read-only GET endpoints are not annotated by the finance-write policy.
- [ ] Identity `/login` and `/register` remain mapped by Identity minimal APIs and receive auth policy through the endpoint convention.

---

## Validation Commands

Validation loop: the plan is not complete until every command below passes (exit 0). On any failure, fix the cause and re-run - loop until all pass. If a check is genuinely impossible, mark it `[f]`, note why in Agent Notes, and move on.

### Level 1: STATIC_ANALYSIS

```bash
dotnet build server/FinancesControl.sln
```

**EXPECT**: Exit 0, no compiler errors or nullable warnings introduced by the change.

### Level 2: UNIT_TESTS

```bash
dotnet test server/FinancesControl.sln --filter ApiRateLimitingConfigurationTests
```

**EXPECT**: New rate-limiting tests pass.

### Level 3: FULL_SUITE

```bash
dotnet test server/FinancesControl.sln
```

**EXPECT**: All backend tests pass.

### Level 4: DATABASE_VALIDATION (if schema changes)

Not applicable. This feature does not change database schema or migrations.

### Level 5: BROWSER_VALIDATION (if UI changes)

Not applicable. This feature has no frontend/UI changes.

### Level 6: MANUAL_VALIDATION

1. Run the API locally.
2. Send more than the configured auth limit to `POST /login` from the same client IP within one window.
3. Confirm successful requests behave as before until the limit is exhausted.
4. Confirm the throttled request returns HTTP `429`, `Content-Type: application/json`, and body fields `httpStatusCode` plus `errors`.
5. Authenticate and send more than the configured finance-write limit to one mutating endpoint, such as `POST /api/Transaction/{accountId}`.
6. Confirm finance write throttling is isolated by authenticated user where different tokens/users are available.

---

## Acceptance Criteria

- [ ] `Program.cs` registers ASP.NET Core rate limiting and enables `UseRateLimiter`.
- [ ] Identity minimal API endpoints are protected by a strict auth policy.
- [ ] Finance write actions are protected by a named finance-write policy.
- [ ] Finance read actions are not throttled by the write policy.
- [ ] Rate-limit rejections return `429` using the existing JSON error response shape.
- [ ] Unit tests cover policy names, partition behavior, and rejection response shape.
- [ ] Level 1-3 validation commands pass with exit 0.

---

## Completion Checklist

- [ ] All tasks completed in dependency order
- [ ] Each task validated immediately after completion
- [ ] Level 1: Static analysis build passes
- [ ] Level 2: Rate-limiting unit tests pass
- [ ] Level 3: Full backend test suite passes
- [ ] Level 4: Database validation not applicable
- [ ] Level 5: Browser validation not applicable
- [ ] All acceptance criteria met

---

## Risks and Mitigations

| Risk | Likelihood | Impact | Mitigation |
| ---- | ---------- | ------ | ---------- |
| Middleware ordering prevents endpoint/user partitioning from working as intended | MED | HIGH | Place `UseRateLimiter` after `UseAuthentication` and before `UseAuthorization`; validate with tests and manual auth/write calls |
| Identity minimal API endpoint convention does not apply as expected | LOW | HIGH | Attach policy directly to the result of `app.MapIdentityApi<IdentityUser>()` and build/test compile against .NET 8 |
| In-memory rate limits do not coordinate across multiple production instances | MED | MED | Document as explicit scope limit; use this as first layer only and consider distributed/proxy rate limiting later |
| Overly strict defaults block legitimate users | MED | MED | Keep defaults conservative but adjustable in `ApiRateLimitOptions`; manual validation should verify normal user flows |
| Rejection response shape differs from existing API error contracts | LOW | MED | Reuse `ErrorResponse`, mirror global exception middleware tests, assert JSON shape |

---

## Questionables

<details>
<summary>Exact production thresholds are not documented yet</summary>

Assumption: use 5 auth attempts per minute per IP and 60 finance write requests per minute per user/IP as initial defaults. These values are intentionally easy to tune in `ApiRateLimitOptions`.

</details>

<details>
<summary>Multi-instance deployment topology is not documented yet</summary>

Assumption: built-in in-memory ASP.NET Core rate limiting is acceptable for this task because the prompt explicitly asks for ASP.NET Core rate limiting policies and the repo has no distributed cache/proxy convention documented.

</details>

---

## Agent Notes

APPROACH_CHOSEN: Centralize policy names, limiter defaults, partition keys, and rejection behavior under `Common/RateLimiting`, wire them in `Program.cs`, and annotate mutating finance controller actions with `[EnableRateLimiting]`.

RATIONALE: This matches the repo's existing common static configuration pattern (`IdentityOptionsConfiguration`) and keeps `Program.cs` readable while avoiding endpoint URL string matching for controller routes. Auth endpoints are generated by `MapIdentityApi<IdentityUser>()`, so the most direct integration is a named policy attached to that endpoint convention builder.

ALTERNATIVES_REJECTED:

- Global limiter only: rejected because the task asks for stricter auth limits and reasonable per-user/per-IP limits for authenticated API routes.
- Path-based global limiter for all POST/PUT/PATCH/DELETE: rejected because it is more fragile than explicit controller action attributes and could accidentally capture non-finance routes.
- Custom middleware: rejected because ASP.NET Core already provides official rate limiting middleware for this exact use case.
- Database-backed counters: rejected because no persistence is needed for the requested app-level throttling and it would introduce unnecessary data access complexity.

External research findings:

- Microsoft Learn says rate limiting middleware is configured with policies and attached to endpoints; `UseRateLimiter` must be called to enable it.
- Microsoft Learn documents fixed window, sliding window, token bucket, and concurrency limiters. Fixed window is enough here because the task needs straightforward request budgets rather than burst smoothing.
- Microsoft Learn documents partitioning by IP address and user identity, which maps directly to public auth and authenticated finance writes.
- Microsoft Learn notes apps using rate limiting should be load tested and reviewed before deploying; include manual checks and future operational review.

No database docs drive implementation here: this feature does not touch Dapper repositories, SQL command classes, or migration scripts.

Confidence score: 8/10. The plan is source-backed and narrow. The main uncertainty is the exact test mechanics for exercising `RateLimiterOptions` named policies because the project has no existing integration-test host, but direct configuration tests should still be feasible.

---

## Amendments

<details>
<summary>2026-07-16T18:38:39.4218398-03:00 - Initial plan</summary>

Created plan from `PRPs/tasks/add-api-rate-limiting.md` after codebase exploration, architecture analysis, and official ASP.NET Core rate limiting documentation review.

</details>

<details>
<summary>2026-07-16T18:55:56.1212393-03:00 - Implementation complete</summary>

Implemented common ASP.NET Core rate limiting configuration, wired it into `Program.cs`, attached the auth policy to the existing restricted Identity endpoint mapping, annotated finance write controller actions, and added focused common infrastructure tests. Deviation: the app already used `MapRestrictedIdentityApi<IdentityUser>()`, so rate limiting was applied to that mapping instead of replacing it with the default Identity API mapping.

</details>
