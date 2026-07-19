# Feature: Add CSRF Strategy For Cookie Authenticated Requests

## Summary

Add a backend antiforgery-token strategy for the cookie-authenticated API and configure the shared frontend Axios client to echo the token on unsafe requests. The plan keeps the current cookie login flow (`POST /login?useCookies=true`) and credentialed CORS policy, adds a small authenticated token endpoint that issues a readable `XSRF-TOKEN` cookie, validates `POST`, `PUT`, `PATCH`, and `DELETE` requests on authenticated controller actions, and documents SameSite/CORS verification as supporting production hardening.

## User Story

As an authenticated finances-control user
I want state-changing requests to require a per-session CSRF token
So that another site cannot silently submit account, bank, card, category, transaction, or logout actions with my cookies

## Problem Statement

The frontend sends cookies on API requests through `withCredentials: true` and signs in through `/login?useCookies=true`, but there is no existing `csrf`, `xsrf`, or `Antiforgery` implementation. If the backend accepts credentialed cross-origin unsafe requests without a non-cookie token check, authenticated users remain exposed to CSRF.

## Solution Statement

Use ASP.NET Core antiforgery services to issue a non-HttpOnly request token in an `XSRF-TOKEN` cookie and validate it through an explicit MVC filter for unsafe authenticated controller requests. Configure Axios v1.17.0 centrally with `xsrfCookieName: 'XSRF-TOKEN'`, `xsrfHeaderName: 'X-XSRF-TOKEN'`, and cross-origin XSRF header sending for unsafe methods. Add integration tests for token issuance, missing-token rejection, valid-token acceptance, logout coverage, and unchanged GET/login behavior.

## Metadata

| Field            | Value |
| ---------------- | ----- |
| Type             | ENHANCEMENT |
| Complexity       | MEDIUM |
| Systems Affected | Backend API startup, common auth/security infrastructure, auth controller, finance controllers, frontend Axios client, frontend auth API, backend integration tests |
| Dependencies     | ASP.NET Core antiforgery in .NET 8 shared framework, Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.23, Microsoft.AspNetCore.Mvc.Testing 8.0.23, axios 1.17.0, React 19.2.6 |
| Estimated Tasks  | 8 |

---

## Lifecycle (append-only)

- **Created:** 2026-07-19T20:29:13.2547709-03:00
- **Modified:**
  - 2026-07-19T20:29:13.2547709-03:00 - Initial plan created from `PRPs/tasks/add-csrf-strategy-for-cookie-authenticated-requests.md`.
  - 2026-07-19T20:43:54.3647942-03:00 - Implemented CSRF strategy and validation tests.
- **Commits:** none yet
- **Agent / Session:**
  - Codex GPT-5 planning session, 2026-07-19
  - Codex GPT-5 implementation session, 2026-07-19
- **Back refs:**
  - `PRPs/tasks/add-csrf-strategy-for-cookie-authenticated-requests.md` - source task
  - `PRPs/plans/completed/restrict-public-authentication-endpoints.plan.md` - existing Identity endpoint wrapping context
  - `PRPs/plans/completed/add-api-rate-limiting.plan.md` - existing auth/write security middleware context
- **Forward refs:** none yet

> **Append-only:** `Created` is set once; every other field is a list you only ever add to - never overwrite or remove existing entries. Keep references bidirectional: when you add a back/forward ref here, add the reciprocal ref on the other plan.

---

## UX Design

### Before State

```text
+------------------------------------------------------------------------------+
| BEFORE STATE                                                                 |
+------------------------------------------------------------------------------+
| Login page -> POST /login?useCookies=true -> auth cookie stored by browser    |
| Authenticated screen -> create/update/delete -> Axios sends cookies only      |
| Backend controller receives authenticated unsafe request and processes it      |
|                                                                              |
| USER_FLOW: The user signs in and uses the app normally.                       |
| PAIN_POINT: Browser cookies are automatic, so a malicious site can attempt    |
| cross-site unsafe requests if cookie/CORS/SameSite defenses are insufficient. |
| DATA_FLOW: component -> hook -> feature API -> axiosClient with cookies -> API |
+------------------------------------------------------------------------------+
```

### After State

```text
+------------------------------------------------------------------------------+
| AFTER STATE                                                                  |
+------------------------------------------------------------------------------+
| Login page -> POST /login?useCookies=true -> auth cookie stored by browser    |
| App fetches /antiforgery/token -> readable XSRF-TOKEN cookie is set           |
| Authenticated mutation -> Axios adds X-XSRF-TOKEN header from cookie          |
| Backend validates token before processing unsafe authenticated actions         |
|                                                                              |
| USER_FLOW: The user signs in and uses the app normally.                       |
| VALUE_ADD: Same visible experience, but forged unsafe requests are rejected.  |
| DATA_FLOW: component -> hook -> feature API -> axiosClient cookie+header ->   |
| antiforgery validation -> controller action                                   |
+------------------------------------------------------------------------------+
```

### Interaction Changes

| Location | Before | After | User Impact |
|----------|--------|-------|-------------|
| `client/finances-control-web/src/api/axiosClient.ts` | Sends cookies, no CSRF header | Sends `X-XSRF-TOKEN` from `XSRF-TOKEN` cookie on unsafe methods | Normal app actions continue; forged requests lack the header |
| `client/finances-control-web/src/features/auth/authApi.ts` | Login only posts credentials | Login flow can request/refresh the CSRF token after successful cookie login | No visible UI change |
| `server/FinancesControl.API/Modules/Auth/Controllers/AuthController.cs` | `POST /logout` signs out with cookie auth only | Logout also requires valid antiforgery token | Cross-site logout attempts are rejected |
| Finance controllers | Mutations require authentication and rate limiting | Mutations require authentication, rate limiting, and antiforgery token | Financial writes gain CSRF protection |

---

## Mandatory Reading

**CRITICAL: Implementation agent MUST read these files before starting any task:**

| Priority | File | Lines | Why Read This |
|----------|------|-------|---------------|
| P0 | `PRPs/tasks/add-csrf-strategy-for-cookie-authenticated-requests.md` | all | Source task and expected direction |
| P0 | `docs/react-typescript-axios-structure.md` | all | Frontend API layering rule: Component -> Hook -> API Function -> Axios Client |
| P0 | `docs/backend-testing-guidelines.md` | all | Backend test naming, style, and validation expectations |
| P0 | `client/finances-control-web/src/api/axiosClient.ts` | 1-10 | Single shared Axios instance and `withCredentials` configuration |
| P0 | `client/finances-control-web/src/features/auth/authApi.ts` | 1-15 | Cookie login, logout, and current-user calls |
| P0 | `server/FinancesControl.API/Program.cs` | 19-115 | Service registration, CORS, Identity mapping, auth middleware order, controllers |
| P1 | `server/FinancesControl.API/Common/IdentityEndpointRouteBuilderExtensions.cs` | 15-37 | Existing common auth endpoint infrastructure pattern |
| P1 | `server/FinancesControl.API/Modules/Auth/Controllers/AuthController.cs` | 7-24 | Root-routed authenticated logout endpoint |
| P1 | `server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs` | 19-167 | Integration-test factory, cookie login, user seeding pattern |
| P1 | `server/FinancesControl.API/Common/ProductionConfiguration.cs` | 43-56 | Existing production CORS origin validation |
| P2 | `client/finances-control-web/package.json` | all | Frontend scripts and Axios version |
| P2 | `server/FinancesControl.API/FinancesControl.API.csproj` | all | Backend package versions and .NET target |

**External Documentation:**
| Source | Section | Why Needed |
|--------|---------|------------|
| [ASP.NET Core antiforgery docs .NET 8/10](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0#javascript-ajax-and-spas) | JavaScript, AJAX, and SPAs | Confirms cookie-auth APIs need a non-cookie token sent as a header |
| [ASP.NET Core antiforgery docs](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0#antiforgery-with-addcontrollers) | Antiforgery with `AddControllers` | Confirms `AddControllers` does not enable antiforgery automatically |
| [ASP.NET Core antiforgery docs](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0#configure-antiforgery-with-antiforgeryoptions) | Configure antiforgery options | Defines header and cookie option names |
| [Axios request config v1.17](https://github.com/axios/axios#request-config) | `xsrfCookieName`, `xsrfHeaderName`, `withXSRFToken` | Confirms Axios can read an XSRF cookie and attach a header, including cross-origin when configured |

---

## Patterns to Mirror

**NAMING_CONVENTION:**

```csharp
// SOURCE: server/FinancesControl.API/Common/IdentityEndpointRouteBuilderExtensions.cs:8-17
// COPY THIS PATTERN:
public static class IdentityEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapRestrictedIdentityApi<TUser>(
        this IEndpointRouteBuilder endpoints,
        bool allowDevelopmentRegistration = false)
        where TUser : class, new()
```

**ERROR_HANDLING:**

```csharp
// SOURCE: server/FinancesControl.API/Common/RateLimiting/ApiRateLimitingConfiguration.cs:60-78
// COPY THIS PATTERN:
var response = new ErrorResponse();
response.SetHttpStatusCode(StatusCodes.Status429TooManyRequests);
response.AddError(RejectionMessage);

context.HttpContext.Response.Clear();
context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
context.HttpContext.Response.ContentType = "application/json";
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

```text
No repository or database changes are needed. Dapper and database table guidelines were read because they are project-wide backend rules, but this feature stays in HTTP security infrastructure.
```

**SERVICE_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Program.cs:64-65
// COPY THIS PATTERN:
builder.Services.AddRateLimiter(options =>
    ApiRateLimitingConfiguration.Configure(options, new ApiRateLimitOptions()));
```

**TEST_STRUCTURE:**

```csharp
// SOURCE: server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs:19-28
// COPY THIS PATTERN:
[Fact]
public async Task Login_WithAnonymousRequest_ReachesIdentityLoginEndpoint()
{
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.PostAsJsonAsync("/login?useCookies=true", new { });

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}
```

**FRONTEND_API_PATTERN:**

```ts
// SOURCE: client/finances-control-web/src/api/axiosClient.ts:1-10
// COPY THIS PATTERN:
import axios from 'axios'

export const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 10000,
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
})
```

---

## Files to Change

| File | Action | Justification |
| ---- | ------ | ------------- |
| `server/FinancesControl.API/Common/Antiforgery/ApiAntiforgeryConfiguration.cs` | CREATE | Centralize token cookie/header names and antiforgery options |
| `server/FinancesControl.API/Common/Antiforgery/ValidateAntiforgeryTokenAttribute.cs` | CREATE | Validate unsafe authenticated controller requests and return project-shaped JSON errors |
| `server/FinancesControl.API/Program.cs` | UPDATE | Register antiforgery services, map token endpoint, and place antiforgery middleware correctly |
| `server/FinancesControl.API/Modules/Auth/Controllers/AuthController.cs` | UPDATE | Require antiforgery validation for `POST /logout` |
| Finance controllers under `server/FinancesControl.API/Modules/*/Controllers` | UPDATE | Require antiforgery validation on data-changing actions or at controller level where safe |
| `client/finances-control-web/src/api/axiosClient.ts` | UPDATE | Configure Axios XSRF cookie/header names and unsafe-method header behavior |
| `client/finances-control-web/src/features/auth/authApi.ts` | UPDATE | Add token-fetch API function and call it after successful cookie login if implementation chooses explicit refresh |
| `server/FinancesControl.Tests/Common/ApiAntiforgeryTests.cs` | CREATE | Integration coverage for token issuance, missing-token rejection, valid-token acceptance, and safe-method behavior |
| `server/FinancesControl.Tests/Common/ApiAntiforgeryConfigurationTests.cs` | CREATE | Focused tests for constants/options if the configuration exposes pure helpers |

---

## NOT Building (Scope Limits)

Explicit exclusions to prevent scope creep:

- No replacement of ASP.NET Core Identity login/current-user endpoints; the app keeps `MapRestrictedIdentityApi<IdentityUser>()`.
- No move from cookie auth to bearer tokens; this task protects the existing cookie-authenticated flow.
- No database or Dapper changes; CSRF state is handled by ASP.NET Core antiforgery, not persisted tables.
- No visible UI changes; token fetch and header attachment are invisible plumbing.
- No broad frontend test framework setup; this repo currently validates frontend changes with `npm run lint` and `npm run build`.
- No SameSite-only strategy as the sole defense unless the backend owner explicitly rejects antiforgery tokens; credentialed cross-origin cookies make token validation the safer default.

---

## Step-by-Step Tasks

Execute in order. Each task is atomic and independently verifiable.

**Status markers** - prefix EVERY task header with one; the build agent updates it inline as it works: `[ ]` idle · `[wip]` in progress · `[x]` complete · `[f]` failed. All tasks start `[ ]`. If a task cannot be made to pass, mark it `[f]`, record why in Agent Notes, and move on if the rest of the plan can still proceed.

### `[x]` Task 1: CREATE `server/FinancesControl.API/Common/Antiforgery/ApiAntiforgeryConfiguration.cs`

- **ACTION**: CREATE a common static configuration class.
- **IMPLEMENT**: Define constants `TokenCookieName = "XSRF-TOKEN"` and `TokenHeaderName = "X-XSRF-TOKEN"`. Add a `Configure(AntiforgeryOptions options)` method that sets `options.HeaderName`, sets `options.Cookie.Name`, marks the antiforgery cookie non-HttpOnly only for the request-token cookie that the SPA must read, sets `SameSite` and `SecurePolicy` deliberately, and keeps names aligned with Axios.
- **MIRROR**: `server/FinancesControl.API/Common/IdentityOptionsConfiguration.cs:5-18` - static common configuration class.
- **IMPORTS**: `using Microsoft.AspNetCore.Antiforgery;`, `using Microsoft.AspNetCore.Http;`
- **GOTCHA**: ASP.NET Core docs note `AddControllers` does not enable antiforgery automatically; this configuration must be wired in `Program.cs`.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 2: CREATE `server/FinancesControl.API/Common/Antiforgery/ValidateAntiforgeryTokenAttribute.cs`

- **ACTION**: CREATE an async authorization filter attribute for MVC controllers.
- **IMPLEMENT**: Resolve `IAntiforgery`, skip safe methods (`GET`, `HEAD`, `OPTIONS`, `TRACE`), call `ValidateRequestAsync(HttpContext)` for unsafe methods, and on `AntiforgeryValidationException` set a JSON `BadRequestObjectResult` or direct response using the existing `ErrorResponse` shape. Use a concise constant message such as `Invalid or missing CSRF token.`.
- **MIRROR**: `server/FinancesControl.API/Common/RateLimiting/ApiRateLimitingConfiguration.cs:60-78` - project JSON error-response shape; `server/FinancesControl.API/Common/Middleware/GlobalExceptionHandlerMiddleware.cs:32-44` - exception/logging style.
- **IMPORTS**: `using Microsoft.AspNetCore.Antiforgery;`, `using Microsoft.AspNetCore.Mvc;`, `using Microsoft.AspNetCore.Mvc.Filters;`
- **GOTCHA**: Microsoft docs for Minimal APIs say antiforgery middleware by itself does not short-circuit all endpoint code paths; an explicit MVC filter gives observable blocking before controller action logic.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 3: UPDATE `server/FinancesControl.API/Program.cs`

- **ACTION**: UPDATE backend startup wiring.
- **IMPLEMENT**: Register antiforgery with `builder.Services.AddAntiforgery(ApiAntiforgeryConfiguration.Configure);`. Map an authorized token endpoint, for example `GET /antiforgery/token`, that calls `IAntiforgery.GetAndStoreTokens(HttpContext)` and appends `XSRF-TOKEN` as a readable cookie. Place `app.UseAntiforgery()` after `app.UseAuthentication()` and `app.UseAuthorization()` if endpoint metadata is used, or document why the custom MVC filter handles validation without relying on middleware short-circuiting. Preserve CORS, Identity mapping, rate limiting, and controller mapping.
- **MIRROR**: `server/FinancesControl.API/Program.cs:27-37` for CORS policy style; `server/FinancesControl.API/Program.cs:99-113` for endpoint/middleware ordering.
- **IMPORTS**: `using Microsoft.AspNetCore.Antiforgery;`, `using FinancesControl.API.Common.Antiforgery;`
- **GOTCHA**: The token endpoint must be callable after login with credentials from the configured frontend origin; do not make it anonymous unless product/security intentionally accepts token issuance before authentication.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 4: UPDATE `server/FinancesControl.API/Modules/Auth/Controllers/AuthController.cs`

- **ACTION**: UPDATE logout protection.
- **IMPLEMENT**: Add the custom antiforgery validation attribute to `Logout()` or the controller in a way that protects `POST /logout` and does not affect unsupported Identity endpoints. Keep `[Authorize]`, `[Route("")]`, and existing `SignOutAsync()` behavior.
- **MIRROR**: `server/FinancesControl.API/Modules/Auth/Controllers/AuthController.cs:7-24` - controller structure and action shape.
- **IMPORTS**: `using FinancesControl.API.Common.Antiforgery;`
- **GOTCHA**: Logout is state-changing and cookie-authenticated, so it must not be skipped just because it is outside `/api/*`.
- **VALIDATE**: `dotnet test server/FinancesControl.sln --filter AuthController`

### `[x]` Task 5: UPDATE finance controller mutation actions

- **ACTION**: UPDATE `AccountController`, `BankController`, `CardController`, `TransactionCategoryController`, and `TransactionController`.
- **IMPLEMENT**: Apply the custom antiforgery validation to every action with `[HttpPost]`, `[HttpPut]`, `[HttpPatch]`, or `[HttpDelete]`. Prefer action-level attributes beside existing `[EnableRateLimiting(RateLimitPolicyNames.FinanceWrite)]` for precision. Do not require tokens for read-only GET endpoints.
- **MIRROR**: `server/FinancesControl.API/Modules/Accounts/Controllers/AccountController.cs:24-92`, `server/FinancesControl.API/Modules/Transactions/Controllers/TransactionController.cs:29-65` - existing mutating action/rate-limit attribute pattern.
- **IMPORTS**: `using FinancesControl.API.Common.Antiforgery;`
- **GOTCHA**: Do not accidentally protect `GET /manage/info` or read endpoints; Axios will only attach the token to unsafe methods.
- **VALIDATE**: `dotnet test server/FinancesControl.sln --filter Controller`

### `[x]` Task 6: UPDATE `client/finances-control-web/src/api/axiosClient.ts`

- **ACTION**: UPDATE shared Axios configuration.
- **IMPLEMENT**: Add `xsrfCookieName: 'XSRF-TOKEN'`, `xsrfHeaderName: 'X-XSRF-TOKEN'`, and `withXSRFToken` configured to return true for unsafe methods (`post`, `put`, `patch`, `delete`). Keep `withCredentials: true`, JSON header, timeout, and base URL unchanged.
- **MIRROR**: `client/finances-control-web/src/api/axiosClient.ts:1-10` - single shared Axios instance pattern.
- **IMPORTS**: Keep `import axios from 'axios'`; add type imports only if TypeScript requires them.
- **GOTCHA**: Axios defaults only attach XSRF headers for same-origin requests; this app uses a Vite frontend origin and API origin, so cross-origin unsafe requests need explicit `withXSRFToken` behavior.
- **VALIDATE**: `cd client/finances-control-web && npm run build`

### `[x]` Task 7: UPDATE `client/finances-control-web/src/features/auth/authApi.ts`

- **ACTION**: UPDATE auth API functions.
- **IMPLEMENT**: Add `getAntiforgeryToken(): Promise<void>` that calls `axiosClient.get('/antiforgery/token')`. After successful `login(payload)`, call `getAntiforgeryToken()` before returning so subsequent mutations have a readable `XSRF-TOKEN` cookie. Leave `getCurrentUser()` returning `response.data`. Keep endpoint URLs inside this API file, not components.
- **MIRROR**: `client/finances-control-web/src/features/auth/authApi.ts:4-15` - feature API style; `docs/react-typescript-axios-structure.md` - API layer responsibilities.
- **IMPORTS**: Existing imports should remain enough unless a shared type is introduced.
- **GOTCHA**: Do not fetch the token before cookie login unless the backend intentionally makes the token endpoint anonymous; the default plan is authenticated token issuance.
- **VALIDATE**: `cd client/finances-control-web && npm run lint && npm run build`

### `[x]` Task 8: CREATE `server/FinancesControl.Tests/Common/ApiAntiforgeryTests.cs`

- **ACTION**: CREATE integration tests.
- **IMPLEMENT**: Use `WebApplicationFactory<Program>` with the in-memory Identity setup from `IdentityEndpointAuthorizationTests`. Cover: authenticated `GET /antiforgery/token` sets `XSRF-TOKEN`; unauthenticated token request returns `401`; authenticated unsafe finance mutation without header returns `400`; authenticated unsafe mutation with token cookie and `X-XSRF-TOKEN` header reaches the action path and is not rejected for CSRF; `POST /logout` without token is rejected; `GET /manage/info` remains unaffected by antiforgery.
- **MIRROR**: `server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs:19-167` - factory, cookie login, seed user pattern; `server/FinancesControl.Tests/Common/GlobalExceptionHandlerMiddlewareTests.cs:22-27` - JSON error response assertions.
- **IMPORTS**: `using System.Net;`, `using System.Net.Http.Json;`, `using FluentAssertions;`, `using Microsoft.AspNetCore.Mvc.Testing;`, plus EF/Identity service override imports.
- **GOTCHA**: Use an endpoint whose service dependencies can be satisfied or substitute/seed enough data that a valid-token request proves antiforgery accepted it even if domain validation later returns a business status.
- **VALIDATE**: `dotnet test server/FinancesControl.sln --filter ApiAntiforgeryTests`

---

## Testing Strategy

### Unit Tests to Write

| Test File | Test Cases | Validates |
| --------- | ---------- | --------- |
| `server/FinancesControl.Tests/Common/ApiAntiforgeryConfigurationTests.cs` | header name, cookie name, readable request-token cookie options | Configuration stays aligned with Axios |
| `server/FinancesControl.Tests/Common/ApiAntiforgeryTests.cs` | token issuance, missing token rejection, valid token acceptance, logout coverage, GET unaffected | Runtime CSRF behavior |

### Edge Cases Checklist

- [ ] Unauthenticated `GET /antiforgery/token` returns `401`.
- [ ] Authenticated unsafe request with no `X-XSRF-TOKEN` header returns `400`.
- [ ] Authenticated unsafe request with malformed token returns `400`.
- [ ] Authenticated unsafe request with matching cookie/header reaches controller logic.
- [ ] `POST /logout` requires antiforgery token.
- [ ] `GET` requests such as `/manage/info` do not require antiforgery token.
- [ ] `POST /login?useCookies=true` still reaches Identity login behavior before any token exists.
- [ ] Production CORS origins remain explicit and wildcard-free.

---

## Validation Commands

**IMPORTANT**: Replace these placeholders with actual commands from the project's package.json/config.

Validation loop: the plan is not complete until every command below passes (exit 0). On any failure, fix the cause and re-run - loop until all pass. If a check is genuinely impossible, mark it `[f]`, note why in Agent Notes, and move on.

### Level 1: STATIC_ANALYSIS

```bash
cd client/finances-control-web && npm run lint
dotnet build server/FinancesControl.sln
```

**EXPECT**: Exit 0, no errors or warnings.

### Level 2: UNIT_TESTS

```bash
dotnet test server/FinancesControl.sln --filter ApiAntiforgery
```

**EXPECT**: New antiforgery tests pass.

### Level 3: FULL_SUITE

```bash
dotnet test server/FinancesControl.sln
cd client/finances-control-web && npm run build
```

**EXPECT**: All backend tests pass and frontend production build succeeds.

### Level 4: DATABASE_VALIDATION (if schema changes)

Not applicable. This plan does not change database schema or SQL scripts.

### Level 5: BROWSER_VALIDATION (if UI changes)

No visible UI changes are expected. If a dev server is already part of the implementation workflow, manually verify login followed by one create/update flow in the browser network panel: `GET /antiforgery/token` sets `XSRF-TOKEN`, and subsequent unsafe requests include `X-XSRF-TOKEN`.

### Level 6: MANUAL_VALIDATION

1. Start the API and frontend with configured CORS origin `http://localhost:5173`.
2. Log in through the frontend.
3. Confirm the browser stores the auth cookie and a readable `XSRF-TOKEN` cookie.
4. Create or update a low-risk record from the UI and confirm the request includes `X-XSRF-TOKEN`.
5. Replay the same unsafe request without the `X-XSRF-TOKEN` header from an HTTP client and confirm the API rejects it.
6. Confirm `GET /manage/info` and normal page refresh auth restoration still work.

---

## Acceptance Criteria

- [ ] Backend issues a readable `XSRF-TOKEN` request-token cookie from an authenticated token endpoint.
- [ ] Axios sends `X-XSRF-TOKEN` for `POST`, `PUT`, `PATCH`, and `DELETE` requests while preserving `withCredentials: true`.
- [ ] Authenticated finance mutations and `POST /logout` reject missing or invalid CSRF tokens.
- [ ] `POST /login?useCookies=true` and `GET /manage/info` continue to work without regressions.
- [ ] Production CORS validation remains explicit and wildcard-free.
- [ ] Level 1-3 validation commands pass with exit 0.
- [ ] Code mirrors existing common configuration, controller attribute, and integration test patterns.

---

## Completion Checklist

- [ ] All tasks completed in dependency order
- [ ] Each task validated immediately after completion
- [ ] Level 1: Static analysis (lint + build) passes
- [ ] Level 2: Antiforgery tests pass
- [ ] Level 3: Full backend test suite + frontend build succeeds
- [ ] Level 4: Database validation marked not applicable
- [ ] Level 5: Browser/network validation completed if running the app locally
- [ ] All acceptance criteria met

---

## Risks and Mitigations

| Risk | Likelihood | Impact | Mitigation |
| ---- | ---------- | ------ | ---------- |
| Middleware/filter ordering allows controller code to run before validation | MED | HIGH | Use an MVC authorization filter for controller actions and integration-test missing-token rejection |
| Axios does not attach XSRF header on cross-origin requests | MED | HIGH | Configure `withXSRFToken` explicitly for unsafe methods and verify with frontend build/manual network check |
| Login is blocked before a token exists | LOW | HIGH | Do not apply antiforgery validation to `POST /login`; fetch token only after successful login |
| Logout omitted from protection | MED | MED | Add explicit task and tests for `POST /logout` missing-token rejection |
| Antiforgery cookie settings break local HTTP development | MED | MED | Follow ASP.NET Core docs on secure policy and set options deliberately for Development vs non-Development if needed |
| Valid-token integration tests fail due unrelated domain prerequisites | MED | LOW | Assert "not rejected by CSRF" using status not equal to the CSRF rejection status/message when domain validation is expected |

---

## Questionables

<details>
<summary>Should the token endpoint be authenticated?</summary>

Assumption: Yes. The plan makes `GET /antiforgery/token` authenticated because the frontend only needs the token after cookie login. If a future anonymous registration or pre-login mutation flow needs CSRF, issue tokens earlier and adjust tests.

</details>

<details>
<summary>Should SameSite-only be accepted as the whole CSRF strategy?</summary>

Assumption: No. SameSite and strict CORS are useful supporting controls, but the current app intentionally sends credentialed cross-origin requests from the Vite frontend to the API. A header token provides clearer protection for authenticated unsafe requests.

</details>

<details>
<summary>Should Identity minimal API endpoints get antiforgery validation?</summary>

Assumption: Only app-supported state-changing endpoints are in scope: login remains unblocked before token issuance, unsupported Identity endpoints remain restricted by existing authorization metadata, and `GET /manage/info` is safe-method current-user lookup.

</details>

---

## Agent Notes

APPROACH_CHOSEN: Backend-issued ASP.NET Core antiforgery request token plus Axios XSRF header attachment for unsafe methods.

RATIONALE: This matches the repo's existing centralization points. Backend security configuration already lives in common helpers and `Program.cs`; frontend HTTP behavior already flows through a single Axios client. ASP.NET Core docs state cookie-auth APIs have CSRF risk and SPA/AJAX requests can send a token via header. Axios v1.17.0 supports `xsrfCookieName`, `xsrfHeaderName`, and `withXSRFToken`, so no new frontend dependency is required.

ALTERNATIVES_REJECTED:

- SameSite/CORS-only: rejected as the primary plan because credentialed cross-origin cookies are an intentional part of the current frontend/backend setup, and SameSite behavior varies with deployment topology.
- Per-feature manual header code: rejected because every feature API imports `axiosClient`; duplicating CSRF reads in feature files would violate the documented API layering.
- Full custom Identity endpoint replacement: rejected because endpoint exposure has already been handled by `MapRestrictedIdentityApi<IdentityUser>()`, and this task is only CSRF protection.

Discovery highlights:

| Category | File:Lines | Pattern Description | Code Snippet |
| -------- | ---------- | ------------------- | ------------ |
| FLOW | `client/finances-control-web/src/api/axiosClient.ts:3-9` | Single Axios instance with credentials | `withCredentials: true` |
| AUTH | `client/finances-control-web/src/features/auth/authApi.ts:4-15` | Cookie login/logout/current-user API file | `await axiosClient.post('/login?useCookies=true', payload)` |
| CORS | `server/FinancesControl.API/Program.cs:27-37` | Configured origins plus credentials | `.WithOrigins(...).AllowCredentials()` |
| IDENTITY | `server/FinancesControl.API/Program.cs:99-100` | Restricted Identity mapping with rate limit | `MapRestrictedIdentityApi<IdentityUser>(...)` |
| TESTS | `server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs:19-167` | Integration tests with in-memory Identity | `WebApplicationFactory<Program>()` |
| CONFIG | `server/FinancesControl.API/Common/ProductionConfiguration.cs:43-56` | Reject empty/wildcard/trailing-slash production CORS origins | `origin == "*"` |

External research:

- Microsoft ASP.NET Core antiforgery docs: `AddControllers` does not enable antiforgery automatically; token validation must be explicitly added for APIs.
- Microsoft ASP.NET Core antiforgery docs: AJAX/SPAs can read a non-HttpOnly request-token cookie and send it as an antiforgery header.
- Axios request config: `withXSRFToken` controls whether Axios reads the XSRF cookie and sets the header for cross-origin browser requests.

Implementation deviation:

- `ApiAntiforgeryConfiguration.Configure` does not set ASP.NET Core's internal antiforgery cookie name to `XSRF-TOKEN`. ASP.NET Core validates a server cookie token plus a request token; using the same cookie for both breaks validation. The implementation uses `.FinancesControl.Antiforgery` as the HttpOnly server cookie and writes the returned request token to the readable `XSRF-TOKEN` cookie for Axios.

---

## Amendments

<details>
<summary>2026-07-19T20:29:13.2547709-03:00 - Initial plan</summary>

Created the implementation plan from `PRPs/tasks/add-csrf-strategy-for-cookie-authenticated-requests.md` using codebase discovery, official ASP.NET Core antiforgery docs, and Axios XSRF configuration docs.

</details>

<details>
<summary>2026-07-19T20:42:55.3837928-03:00 - Implemented CSRF strategy</summary>

Implemented antiforgery configuration, token endpoint, MVC validation attribute, backend controller coverage, Axios XSRF configuration, post-login token refresh, and backend integration/configuration tests. Deviated from the original cookie-name detail by separating ASP.NET Core's internal antiforgery cookie from the readable SPA request-token cookie so validation works correctly.

</details>
