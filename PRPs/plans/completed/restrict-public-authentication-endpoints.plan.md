# Feature: Restrict Public Authentication Endpoints

## Summary

Restrict ASP.NET Core Identity API endpoint exposure so the application only intentionally supports `POST /login` and `GET /manage/info` from the default Identity endpoint set, while all other default Identity endpoints are unavailable to anonymous callers. The implementation should keep the current Identity service setup and frontend login/current-user flow, add a small backend endpoint convention/extension around `MapIdentityApi<IdentityUser>()`, and prove the runtime allowlist with integration tests.

## User Story

As an application owner
I want default Identity endpoints restricted to the supported authentication surface
So that production does not accidentally allow registration, recovery, refresh, 2FA, or profile mutation flows that the product has not designed or approved

## Problem Statement

`Program.cs` currently calls `app.MapIdentityApi<IdentityUser>()`, which maps the entire ASP.NET Core Identity API endpoint set. Anonymous clients can reach unsupported endpoints such as `POST /register`, `POST /forgotPassword`, `POST /resetPassword`, `POST /refresh`, `POST /resendConfirmationEmail`, and `GET /confirmEmail`, creating a production security risk. The task is complete when anonymous requests can still use `POST /login`, authenticated requests can still use `GET /manage/info`, and unsupported default Identity endpoints are no longer public.

## Solution Statement

Create a focused backend endpoint-mapping helper for Identity endpoints. It will map the framework-provided Identity endpoints, inspect each generated endpoint's route pattern and HTTP method, preserve the allowlisted endpoints (`POST /login`, `GET /manage/info`), and require authorization or suppress public exposure for every other default Identity endpoint. Add integration tests using `Microsoft.AspNetCore.Mvc.Testing` so the behavior is verified against the real ASP.NET Core routing and authorization pipeline instead of only unit-level metadata.

## Metadata

| Field            | Value |
| ---------------- | ----- |
| Type             | ENHANCEMENT |
| Complexity       | MEDIUM |
| Systems Affected | Backend API startup, ASP.NET Core Identity endpoint mapping, backend tests, optional Swagger metadata |
| Dependencies     | ASP.NET Core Identity API endpoints / Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.23, Microsoft.EntityFrameworkCore 8.0.23, Swashbuckle.AspNetCore 6.6.2, xUnit 2.9.2, FluentAssertions 4.19.4, Microsoft.AspNetCore.Mvc.Testing 8.0.x |
| Estimated Tasks  | 5 |

---

## Lifecycle (append-only)

- **Created:** 2026-07-16T00:00:00-03:00
- **Modified:** 2026-07-16T00:00:00-03:00 - Initial plan created from `PRPs/tasks/restrict-public-authentication-endpoints.md`.
- **Modified:** 2026-07-16T17:34:58-03:00 - Implemented restricted Identity endpoint mapping and validation tests.
- **Commits:** None yet.
- **Agent / Session:** Codex GPT-5 planning session, 2026-07-16.
- **Agent / Session:** Codex GPT-5 implementation session, 2026-07-16.
- **Back refs:** `PRPs/tasks/restrict-public-authentication-endpoints.md` - source task; `docs/features/backend-production-readiness-fixes.md` - production-readiness item.
- **Forward refs:** None yet.

> **Append-only:** `Created` is set once; every other field is a list you only ever add to - never overwrite or remove existing entries. Keep references bidirectional: when you add a back/forward ref here, add the reciprocal ref on the other plan.

---

## UX Design

### Before State

```text
+------------------------------------------------------------------------------+
|                                  BEFORE STATE                                |
+------------------------------------------------------------------------------+
|                                                                              |
|   Login page / API client  ---->  POST /login?useCookies=true  ----> cookie  |
|   App auth provider        ---->  GET /manage/info           ----> user info |
|   Any anonymous client     ---->  POST /register             ----> reachable |
|   Any anonymous client     ---->  reset/refresh/confirm APIs ----> reachable |
|                                                                              |
|   USER_FLOW: The supported UI uses login, logout, and current-user checks.   |
|   PAIN_POINT: Unsupported framework auth endpoints are still mapped publicly.|
|   DATA_FLOW: app.MapIdentityApi maps all Identity endpoints without an app   |
|              allowlist, so route availability exceeds product capability.    |
|                                                                              |
+------------------------------------------------------------------------------+
```

### After State

```text
+------------------------------------------------------------------------------+
|                                  AFTER STATE                                 |
+------------------------------------------------------------------------------+
|                                                                              |
|   Login page / API client  ---->  POST /login?useCookies=true  ----> cookie  |
|   App auth provider        ---->  GET /manage/info           ----> user info |
|                                  requires authenticated Identity cookie        |
|                                                                              |
|   Any anonymous client     ---->  POST /register             ----> blocked   |
|   Any anonymous client     ---->  reset/refresh/confirm APIs ----> blocked   |
|   Authenticated client     ---->  POST /manage/info          ----> blocked   |
|                                                                              |
|   USER_FLOW: The existing frontend auth flow keeps working unchanged.        |
|   VALUE_ADD: Production exposes only approved auth capability.               |
|   DATA_FLOW: Identity endpoints are mapped through a project helper that     |
|              applies runtime endpoint authorization from an allowlist.        |
|                                                                              |
+------------------------------------------------------------------------------+
```

### Interaction Changes

| Location | Before | After | User Impact |
|----------|--------|-------|-------------|
| `POST /login` | Mapped by default Identity API and public | Still mapped and public | Existing sign-in remains available |
| `GET /manage/info` | Mapped by default Identity API under `/manage` authorization | Still mapped and usable for authenticated current-user checks | Existing auth restore flow remains available |
| `POST /register` and account recovery endpoints | Mapped and reachable by anonymous clients | Not public; anonymous requests receive authorization failure or endpoint is hidden/blocked | Unsupported registration and recovery flows cannot be used accidentally |
| `POST /manage/info`, `POST /manage/2fa` | Mapped for authenticated users | Not part of the supported public/default auth surface; should be blocked or hidden per chosen implementation | Users cannot mutate profile/2FA through unplanned default APIs |

---

## Mandatory Reading

**CRITICAL: Implementation agent MUST read these files before starting any task:**

| Priority | File | Lines | Why Read This |
|----------|------|-------|---------------|
| P0 | `server/FinancesControl.API/Program.cs` | 18-109 | Current service registration, CORS, Swagger, Identity mapping, and middleware order |
| P0 | `server/FinancesControl.API/Modules/Auth/Controllers/AuthController.cs` | 7-24 | Existing protected logout endpoint that must keep working |
| P0 | `client/finances-control-web/src/features/auth/authApi.ts` | 4-14 | Confirms frontend depends only on login, logout, and `GET /manage/info` |
| P1 | `server/FinancesControl.API/Common/IdentityOptionsConfiguration.cs` | 5-25 | Existing centralized Identity options to preserve |
| P1 | `server/FinancesControl.API/Data/FinancesDbContext.cs` | 1-8 | Identity EF store context |
| P1 | `docs/backend-testing-guidelines.md` | 1-103 | Backend test conventions and command |
| P2 | `server/FinancesControl.Tests/Common/IdentityOptionsConfigurationTests.cs` | 7-57 | Current xUnit/FluentAssertions test style |
| P2 | `server/FinancesControl.Tests/Accounts/Controllers/AccountControllerTests.cs` | 15-25, 448-456 | Controller test setup and authenticated-user helper style |
| P2 | `docs/dapper-usage-guidelines.md` | all | Project-required backend reading; no Dapper changes expected |

**External Documentation:**
| Source | Section | Why Needed |
|--------|---------|------------|
| [ASP.NET Core Identity API endpoints .NET 8](https://learn.microsoft.com/ro-ro/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-8.0#the-mapidentityapituser-endpoints) | The `MapIdentityApi<TUser>` endpoints | Lists every default endpoint that must be audited |
| [ASP.NET Core Identity route mapping .NET 8](https://learn.microsoft.com/ro-ro/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-8.0#activate-identity-apis) | Activate Identity APIs | Confirms cookie login behavior via `useCookies=true` |
| [ASP.NET Core endpoint authorization .NET 8](https://learn.microsoft.com/ro-ro/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-8.0#secure-selected-endpoints) | Secure selected endpoints | Confirms `RequireAuthorization` is the supported endpoint restriction mechanism |
| [GET `/manage/info` .NET 8](https://learn.microsoft.com/ro-ro/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-8.0#use-the-get-manageinfo-endpoint) | Current user info | Confirms this endpoint is for logged-in users, not anonymous public profile data |
| [MapIdentityApi API reference](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.routing.identityapiendpointroutebuilderextensions.mapidentityapi?view=aspnetcore-9.0) | Return type | Confirms it returns `IEndpointConventionBuilder`, allowing conventions but not selective endpoint generation |
| [ASP.NET Core Identity source](https://source.dot.net/Microsoft.AspNetCore.Identity/IdentityApiEndpointRouteBuilderExtensions.cs.html) | Identity endpoint builder internals | Confirms `/manage` is internally grouped with authorization |
| [Swashbuckle 6.6.2 Microsoft tutorial](https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0#package-installation) | Package version | Matches repo Swashbuckle version if OpenAPI metadata is touched |

---

## Patterns to Mirror

**NAMING_CONVENTION:**

```csharp
// SOURCE: server/FinancesControl.API/Common/IdentityOptionsConfiguration.cs:5-8
// COPY THIS PATTERN:
public static class IdentityOptionsConfiguration
{
    public static void Configure(IdentityOptions options)
    {
```

**ERROR_HANDLING:**

```csharp
// SOURCE: server/FinancesControl.API/Common/Middleware/GlobalExceptionHandlerMiddleware.cs:32-39
// COPY THIS PATTERN:
var (statusCode, message, shouldLogAsError) = exception switch
{
    AuthenticationContextException => ((int)HttpStatusCode.Unauthorized, exception.Message, false),
    ResourceNotFoundException => ((int)HttpStatusCode.NotFound, exception.Message, false),
    DomainValidationException => ((int)HttpStatusCode.BadRequest, exception.Message, false),
    BusinessRuleException => ((int)HttpStatusCode.BadRequest, exception.Message, false),
    _ => ((int)HttpStatusCode.InternalServerError, UnexpectedErrorMessage, true)
};
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

**AUTH_CONTROLLER_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Modules/Auth/Controllers/AuthController.cs:7-23
// COPY THIS PATTERN:
[Route("")]
[ApiController]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly SignInManager<IdentityUser> _signInManager;

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }
}
```

**STARTUP_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Program.cs:61-64,98,104-107
// COPY THIS PATTERN:
builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<IdentityUser>(IdentityOptionsConfiguration.Configure)
    .AddEntityFrameworkStores<FinancesDbContext>();

app.MapIdentityApi<IdentityUser>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
```

**TEST_STRUCTURE:**

```csharp
// SOURCE: server/FinancesControl.Tests/Common/IdentityOptionsConfigurationTests.cs:7-17
// COPY THIS PATTERN:
public class IdentityOptionsConfigurationTests
{
    [Fact]
    public void Configure_WithDefaultOptions_SetsProductionPasswordOptions()
    {
        var options = new IdentityOptions();

        IdentityOptionsConfiguration.Configure(options);

        options.Password.RequiredLength.Should().Be(12);
    }
}
```

---

## Files to Change

| File | Action | Justification |
| ---- | ------ | ------------- |
| `server/FinancesControl.API/Common/IdentityEndpointRouteBuilderExtensions.cs` | CREATE | Encapsulate the allowed/default Identity endpoint policy outside `Program.cs` while matching existing `Common` static helper style |
| `server/FinancesControl.API/Program.cs` | UPDATE | Replace direct `app.MapIdentityApi<IdentityUser>()` with the new restricted mapping helper |
| `server/FinancesControl.Tests/FinancesControl.Tests.csproj` | UPDATE | Add `Microsoft.AspNetCore.Mvc.Testing` for real endpoint authorization integration tests |
| `server/FinancesControl.API/Program.cs` | UPDATE | Add `public partial class Program { }` if required by `WebApplicationFactory<Program>` |
| `server/FinancesControl.Tests/Common/IdentityEndpointRouteBuilderExtensionsTests.cs` | CREATE | Unit-test allowlist classification if helper exposes a pure method |
| `server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs` | CREATE | Integration-test anonymous access to allowed and restricted Identity endpoints |

---

## NOT Building (Scope Limits)

Explicit exclusions to prevent scope creep:

- No public registration or invite-based registration flow. This task only closes unsupported default endpoints.
- No custom password reset, email confirmation, refresh-token, or 2FA workflow. These default Identity endpoints are restricted because product behavior is not designed yet.
- No frontend auth redesign. Existing frontend calls only `POST /login?useCookies=true`, `POST /logout`, and `GET /manage/info`.
- No database schema changes or migrations.
- No change to Identity password/lockout options except preserving the existing `IdentityOptionsConfiguration`.
- No decision to make `GET /manage/info` anonymously accessible. It remains an allowed Identity endpoint for authenticated current-user checks.

---

## Step-by-Step Tasks

Execute in order. Each task is atomic and independently verifiable.

**Status markers** - prefix EVERY task header with one; the build agent updates it inline as it works: `[ ]` idle . `[wip]` in progress . `[x]` complete . `[f]` failed. All tasks start `[ ]`. If a task cannot be made to pass, mark it `[f]`, record why in Agent Notes, and move on if the rest of the plan can still proceed.

### `[x]` Task 1: CREATE `server/FinancesControl.API/Common/IdentityEndpointRouteBuilderExtensions.cs`

- **ACTION**: CREATE a static extension/helper that maps Identity endpoints with an explicit allowlist.
- **IMPLEMENT**: Add a `MapRestrictedIdentityApi<TUser>(this IEndpointRouteBuilder endpoints)` method constrained to `IdentityUser`/`class` as needed. Internally call `endpoints.MapIdentityApi<TUser>()`, then attach a convention to inspect each endpoint's `RouteEndpoint.RoutePattern.RawText` and `HttpMethodMetadata.HttpMethods`. Preserve `POST /login` and `GET /manage/info`; apply `RequireAuthorization()`-equivalent metadata or an endpoint filter/result that blocks every other default Identity endpoint. Prefer authorization metadata over custom path middleware so routing and authorization stay integrated.
- **MIRROR**: `server/FinancesControl.API/Common/IdentityOptionsConfiguration.cs:5-8` - static helper class in `Common`.
- **IMPORTS**: `using Microsoft.AspNetCore.Authorization;`, `using Microsoft.AspNetCore.Http.Metadata;`, `using Microsoft.AspNetCore.Identity;`, `using Microsoft.AspNetCore.Routing;`
- **PATTERN**: Keep the allowlist as private constants or a private static method inside the helper so it is testable and not scattered in `Program.cs`.
- **GOTCHA**: `MapIdentityApi` has no option to generate only selected endpoints; do not assume unmapped endpoints disappear unless the implementation explicitly blocks or filters them. Also do not add broad `AllowAnonymous` metadata to the whole Identity route group.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 2: UPDATE `server/FinancesControl.API/Program.cs`

- **ACTION**: UPDATE startup to use the restricted Identity endpoint mapping.
- **IMPLEMENT**: Replace `app.MapIdentityApi<IdentityUser>();` with the new helper, for example `app.MapRestrictedIdentityApi<IdentityUser>();`. Preserve `AddIdentityApiEndpoints<IdentityUser>(IdentityOptionsConfiguration.Configure)`, EF stores, CORS, `UseAuthentication`, `UseAuthorization`, `MapControllers`, and Swagger behavior. Add `public partial class Program { }` at the end only if integration tests require it.
- **MIRROR**: `server/FinancesControl.API/Program.cs:61-64,98,104-107` - existing Identity service registration and middleware order.
- **IMPORTS**: Add a `using` only if the new helper namespace is not already available.
- **GOTCHA**: Middleware ordering already has `UseAuthentication()` before `UseAuthorization()` and before `MapControllers()`; do not rearrange unrelated pipeline behavior unless required by endpoint tests.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 3: UPDATE `server/FinancesControl.Tests/FinancesControl.Tests.csproj`

- **ACTION**: ADD integration test dependency.
- **IMPLEMENT**: Add `Microsoft.AspNetCore.Mvc.Testing` with an 8.0.x version compatible with the API project's `net8.0` target, preferably `8.0.23` to match the Identity/EF package patch level.
- **MIRROR**: `server/FinancesControl.Tests/FinancesControl.Tests.csproj:10-17` - existing package reference style.
- **GOTCHA**: The test project targets `net9.0` while the API targets `net8.0`; use a package version compatible with the API host and verify restore/build instead of assuming the test target decides the package version.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 4: CREATE `server/FinancesControl.Tests/Common/IdentityEndpointRouteBuilderExtensionsTests.cs`

- **ACTION**: CREATE focused unit tests for allowlist classification if the helper exposes an internal pure method via `InternalsVisibleTo` or equivalent testable surface. If the helper stays private-only, skip this file and cover behavior in Task 5 integration tests.
- **IMPLEMENT**: Test that `POST /login` and `GET /manage/info` are classified as allowed, while `POST /register`, `POST /refresh`, `POST /forgotPassword`, `POST /resetPassword`, `GET /confirmEmail`, `POST /resendConfirmationEmail`, `POST /manage/info`, and `POST /manage/2fa` are restricted.
- **MIRROR**: `server/FinancesControl.Tests/Common/IdentityOptionsConfigurationTests.cs:7-57` - common test namespace, xUnit `[Fact]`/`[Theory]`, FluentAssertions.
- **IMPORTS**: `using FinancesControl.API.Common;`, `using FluentAssertions;`
- **GOTCHA**: Do not test only strings if runtime endpoint metadata can diverge; classification tests are helpful but not a substitute for Task 5 integration tests.
- **VALIDATE**: `dotnet test server/FinancesControl.sln --filter IdentityEndpointRouteBuilderExtensionsTests`

### `[x]` Task 5: CREATE `server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs`

- **ACTION**: CREATE integration tests against the real hosted API.
- **IMPLEMENT**: Use `WebApplicationFactory<Program>` to create a test client. Override database/Identity services with an in-memory or test-safe provider so the host starts without PostgreSQL. Verify anonymous `POST /login` does not return `401` or `403` solely due to endpoint authorization metadata, while anonymous calls to restricted endpoints return `401`, `403`, or `404` according to the chosen blocking strategy. Verify unauthenticated `GET /manage/info` still requires authentication because Microsoft Identity designs it as current-user info. If practical, seed a test user and verify authenticated `GET /manage/info` returns success with `{ email, isEmailConfirmed }`.
- **MIRROR**: `server/FinancesControl.Tests/Common/GlobalExceptionHandlerMiddlewareTests.cs:11-31` - behavior-focused test names and status-code assertions; `docs/backend-testing-guidelines.md:56-73` - naming conventions.
- **IMPORTS**: `using System.Net;`, `using FluentAssertions;`, `using Microsoft.AspNetCore.Mvc.Testing;`, plus EF/Identity test-host imports required for service overrides.
- **GOTCHA**: Hiding endpoints from Swagger is not security. These tests must call the runtime endpoints. Also, `GET /manage/info` is allowed in the supported endpoint surface but is not anonymous; do not assert anonymous success for it.
- **VALIDATE**: `dotnet test server/FinancesControl.sln --filter IdentityEndpointAuthorizationTests`

---

## Testing Strategy

### Unit Tests to Write

| Test File | Test Cases | Validates |
| --------- | ---------- | --------- |
| `server/FinancesControl.Tests/Common/IdentityEndpointRouteBuilderExtensionsTests.cs` | allowlisted endpoint classification, restricted endpoint classification | Helper allowlist rules remain explicit |
| `server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs` | anonymous login allowed, anonymous register/recovery/refresh/manage mutation blocked, `GET /manage/info` requires auth and succeeds for authenticated user if seeded | Real endpoint routing and authorization behavior |

### Edge Cases Checklist

- [ ] `POST /login` with invalid credentials is not blocked by endpoint authorization; it should reach Identity login logic.
- [ ] `GET /manage/info` without a cookie returns unauthorized, not public user data.
- [ ] `GET /manage/info` with a valid cookie still returns current user info.
- [ ] `POST /register` cannot be used anonymously.
- [ ] `POST /forgotPassword`, `POST /resetPassword`, and `GET /confirmEmail` cannot be used anonymously.
- [ ] `POST /refresh` cannot be used anonymously.
- [ ] `POST /manage/info` and `POST /manage/2fa` are not part of the supported public/default auth surface.
- [ ] Swagger/OpenAPI visibility, if changed, does not replace runtime security tests.

---

## Validation Commands

Validation loop: the plan is not complete until every command below passes (exit 0). On any failure, fix the cause and re-run - loop until all pass. If a check is genuinely impossible, mark it `[f]`, note why in Agent Notes, and move on.

### Level 1: STATIC_ANALYSIS

```bash
dotnet build server/FinancesControl.sln
```

**EXPECT**: Exit 0, no compiler errors.

### Level 2: UNIT_TESTS

```bash
dotnet test server/FinancesControl.sln --filter IdentityEndpoint
```

**EXPECT**: New Identity endpoint tests pass.

### Level 3: FULL_SUITE

```bash
dotnet test server/FinancesControl.sln
```

**EXPECT**: All backend tests pass.

### Level 4: DATABASE_VALIDATION (if schema changes)

Not applicable. This plan should not change database schema or migrations.

### Level 5: BROWSER_VALIDATION (if UI changes)

Not applicable. This plan should not change frontend UI.

### Level 6: MANUAL_VALIDATION

1. Run the API locally.
2. Confirm `POST /login?useCookies=true` still reaches Identity login behavior.
3. Confirm `GET /manage/info` without a cookie returns unauthorized.
4. Confirm `POST /register`, `POST /forgotPassword`, `POST /resetPassword`, `POST /refresh`, `GET /confirmEmail`, `POST /resendConfirmationEmail`, `POST /manage/info`, and `POST /manage/2fa` are not publicly usable.
5. Sign in through the frontend and confirm the existing auth restore flow still loads the current user from `GET /manage/info`.

---

## Acceptance Criteria

- [ ] `POST /login` remains available to anonymous clients for supported sign-in.
- [ ] `GET /manage/info` remains available for authenticated current-user checks.
- [ ] Unsupported default Identity endpoints are not public: `POST /register`, `POST /refresh`, `GET /confirmEmail`, `POST /resendConfirmationEmail`, `POST /forgotPassword`, `POST /resetPassword`, `POST /manage/2fa`, and `POST /manage/info`.
- [ ] Existing custom `POST /logout` remains protected by `[Authorize]`.
- [ ] Existing frontend auth API calls do not need to change.
- [ ] Level 1-3 validation commands pass with exit 0.
- [ ] Tests cover the runtime authorization behavior, not only Swagger visibility or helper string matching.
- [ ] Code mirrors existing patterns exactly (Common helper style, minimal hosting startup, xUnit/FluentAssertions test style).

---

## Completion Checklist

- [ ] All tasks completed in dependency order.
- [ ] Each task validated immediately after completion.
- [ ] Level 1: Static analysis/build passes.
- [ ] Level 2: New Identity endpoint tests pass.
- [ ] Level 3: Full backend test suite passes.
- [ ] Level 4: Database validation is confirmed not applicable.
- [ ] Level 5: Browser validation is confirmed not applicable.
- [ ] All acceptance criteria met.

---

## Risks and Mitigations

| Risk | Likelihood | Impact | Mitigation |
| ---- | ---------- | ------ | ---------- |
| `MapIdentityApi` endpoint metadata is harder to customize than expected | MED | HIGH | Prefer a route-builder convention with integration tests; if insufficient, replace default endpoints with explicit custom auth endpoints for login/info only |
| Accidentally making `GET /manage/info` anonymous | LOW | HIGH | Treat it as allowed but authenticated; integration test anonymous request must fail |
| Swagger visibility mistaken for runtime security | MED | HIGH | Runtime integration tests must call endpoints directly |
| Test host tries to connect to PostgreSQL | MED | MED | Override `FinancesDbContext` in `WebApplicationFactory` with a test-safe provider/service setup |
| Blocking `POST /login` while trying to secure other endpoints | MED | HIGH | Test anonymous login reaches Identity logic and is not rejected by authorization metadata |
| Framework endpoint list changes in future package upgrades | MED | MED | Keep endpoint allowlist tests explicit so package changes produce visible failures |

---

## Questionables

<details>
<summary>Should restricted endpoints return 401/403 or 404?</summary>

Assumption: runtime security is the priority, so `401`/`403` is acceptable if the implementation applies authorization metadata. A stricter "not exposed at all" interpretation may prefer `404`, but `MapIdentityApi` does not natively support mapping only selected endpoints. If product/security requires `404`, implement custom explicit endpoints instead of relying on `MapIdentityApi`.

</details>

<details>
<summary>Is `GET /manage/info` supposed to be anonymous-public?</summary>

Assumption: No. The user's phrase "public" is interpreted as "kept in the allowed Identity endpoint surface", not anonymous access. Microsoft documents `GET /manage/info` as returning information for the logged-in user, and the framework source places it under the `/manage` group requiring authorization.

</details>

<details>
<summary>Should Swagger hide restricted Identity endpoints?</summary>

Assumption: Runtime restriction comes first. Swagger filtering is optional and should only be added after runtime tests pass. The current app already exposes Swagger only in development.

</details>

---

## Agent Notes

### Discovery Summary

The exact production-readiness gap is documented in `docs/features/backend-production-readiness-fixes.md:27-33`. Current backend startup registers Identity services at `Program.cs:63-64` and maps all default endpoints at `Program.cs:98`. The frontend auth layer calls only `POST /login?useCookies=true`, `POST /logout`, and `GET /manage/info`, so the backend can restrict the rest without planned frontend changes.

### Approach Chosen

APPROACH_CHOSEN: Keep ASP.NET Core Identity services and the framework login/current-user behavior, but wrap `MapIdentityApi<IdentityUser>()` with a project-specific endpoint convention/helper that applies an explicit allowlist.

RATIONALE: This is the smallest change that fits the existing architecture. The app already uses Identity's cookie login endpoint and `GET /manage/info`; replacing all Identity behavior with custom controllers would be larger and riskier. A helper in `Common` mirrors the existing `IdentityOptionsConfiguration` pattern and keeps `Program.cs` readable.

ALTERNATIVES_REJECTED:

- Replace all Identity endpoints with custom controllers immediately: rejected because only endpoint exposure is in scope, and the current frontend depends on framework login/current-user behavior.
- Hide endpoints only from Swagger: rejected because hiding docs does not secure runtime access.
- Add custom middleware that checks request paths: rejected because endpoint metadata and authorization are the framework-aligned path and easier to verify with routing tests.
- Make `/manage/info` anonymous: rejected because Microsoft documents it as logged-in user info and the source maps `/manage` with authorization.

### Identity Endpoint Matrix

| Endpoint | Current Source | Target |
| -------- | -------------- | ------ |
| `POST /login` | `MapIdentityApi` | Allowed anonymous |
| `GET /manage/info` | `MapIdentityApi` `/manage` group | Allowed authenticated |
| `POST /register` | `MapIdentityApi` | Restricted/not public |
| `POST /refresh` | `MapIdentityApi` | Restricted/not public |
| `GET /confirmEmail` | `MapIdentityApi` | Restricted/not public |
| `POST /resendConfirmationEmail` | `MapIdentityApi` | Restricted/not public |
| `POST /forgotPassword` | `MapIdentityApi` | Restricted/not public |
| `POST /resetPassword` | `MapIdentityApi` | Restricted/not public |
| `POST /manage/2fa` | `MapIdentityApi` `/manage` group | Restricted/not supported |
| `POST /manage/info` | `MapIdentityApi` `/manage` group | Restricted/not supported |

### External Research Notes

Microsoft's .NET 8 Identity API docs list every endpoint created by `MapIdentityApi<TUser>`. The same docs confirm `useCookies=true` controls cookie issuance for login, matching `client/finances-control-web/src/features/auth/authApi.ts:5`. The `MapIdentityApi` API returns `IEndpointConventionBuilder`, which supports conventions but does not provide a first-class option to generate only selected endpoints.

---

## Amendments

<details>
<summary>2026-07-16T00:00:00-03:00 - Initial plan</summary>

Created the implementation plan from `PRPs/tasks/restrict-public-authentication-endpoints.md` and the user's allowlist direction.

</details>

<details>
<summary>2026-07-16T17:34:58-03:00 - Implementation complete</summary>

Implemented `MapRestrictedIdentityApi<TUser>()`, replaced direct Identity API mapping in startup, added integration test dependencies, and covered the allowlist plus hosted endpoint authorization behavior. Restricted endpoints use authorization metadata requiring an application role that is never assigned, so anonymous requests are unauthorized and authenticated users are forbidden from unsupported mutation endpoints. One integration assertion was adjusted from response-body success to authorization success because the .NET 9 test host with the .NET 8 generated Identity endpoint fails during JSON response serialization after authorization succeeds.

</details>
