# Feature: Development Register Endpoint Access

## Summary

Allow the ASP.NET Core Identity `POST /register` endpoint to stay public only in the Development environment so the project developer can create local test users. The implementation should extend the existing restricted Identity endpoint helper with an environment-aware allow-list and update the existing Identity endpoint tests to prove Development access and non-Development restriction.

## User Story

As the sole project developer
I want to register users through the API while running the app in Development
So that I can log into authenticated frontend pages and manually validate frontend fixes

## Problem Statement

`POST /register` is currently restricted in every environment by `MapRestrictedIdentityApi<TUser>()`. That protects production, but it blocks local test-user creation during development and makes authenticated frontend testing harder.

## Solution Statement

Keep using the current `MapIdentityApi<TUser>()` wrapper and endpoint convention. Pass the current environment state into the helper from `Program.cs`, then treat `POST /register` as an allowed Identity endpoint only when the environment is Development. Preserve the existing restrictions for register in Testing and production-like environments, and keep every other hidden Identity endpoint restricted everywhere.

## Metadata

| Field | Value |
| ----- | ----- |
| Type | ENHANCEMENT |
| Complexity | LOW |
| Systems Affected | Backend API startup, ASP.NET Core Identity endpoint mapping, backend tests |
| Dependencies | Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.23, Microsoft.AspNetCore.Mvc.Testing 8.0.23, Microsoft.EntityFrameworkCore.InMemory 8.0.23, xUnit 2.9.2, FluentAssertions 4.19.4 |
| Estimated Tasks | 4 |

---

## Lifecycle (append-only)

- **Created:** 2026-07-19T16:35:00-03:00
- **Modified:** 2026-07-19T16:35:00-03:00 - Initial plan created from `PRPs/tasks/development-register-endpoint-access.md`.
- **Modified:** 2026-07-19T16:55:00-03:00 - Implemented Development-only register endpoint access and validation tests.
- **Commits:** None yet.
- **Agent / Session:** Codex GPT-5 planning session, 2026-07-19.
- **Agent / Session:** Codex GPT-5 implementation session, 2026-07-19.
- **Back refs:** `PRPs/tasks/development-register-endpoint-access.md` - source task; `PRPs/prds/development-register-endpoint-access.prd.md` - source PRD; `PRPs/plans/completed/restrict-public-authentication-endpoints.plan.md` - prior Identity endpoint restriction decision.
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
|   Developer/API client  ---->  POST /register  ---->  401 Unauthorized       |
|   Developer/frontend    ---->  Login page      ---->  No local test user     |
|                                                                              |
|   USER_FLOW: The developer starts frontend work, needs an authenticated user, |
|              but the generated Identity register endpoint is blocked.         |
|   PAIN_POINT: Authenticated frontend pages cannot be reached without another  |
|               manual user-creation workaround.                               |
|   DATA_FLOW: MapIdentityApi creates /register, MapRestrictedIdentityApi adds  |
|              restrictive role metadata because /register is not allowed.      |
|                                                                              |
+------------------------------------------------------------------------------+
```

### After State

```text
+------------------------------------------------------------------------------+
|                                   AFTER STATE                                |
+------------------------------------------------------------------------------+
|                                                                              |
|   Development API client  ---->  POST /register  ---->  Identity user saved  |
|   Developer/frontend      ---->  POST /login     ---->  Authenticated pages  |
|                                                                              |
|   Non-development client  ---->  POST /register  ---->  Still restricted     |
|   Any environment client  ---->  Other hidden Identity endpoints restricted  |
|                                                                              |
|   USER_FLOW: The developer registers a local test user in Development, logs   |
|              in, and validates authenticated frontend behavior.               |
|   VALUE_ADD: Local frontend fixes can be tested end to end without exposing   |
|              production signup.                                              |
|   DATA_FLOW: Program passes environment intent into the Identity endpoint     |
|              helper; only Development allows POST /register through.          |
|                                                                              |
+------------------------------------------------------------------------------+
```

### Interaction Changes

| Location | Before | After | User Impact |
|----------|--------|-------|-------------|
| `POST /register` in Development | Anonymous requests are unauthorized | Anonymous requests reach Identity registration | Developer can create local test users |
| `POST /register` in Testing and non-Development | Anonymous requests are unauthorized | Anonymous requests remain unauthorized | Production safety decision remains intact |
| Other default Identity endpoints | Restricted by unsupported role metadata | Still restricted by unsupported role metadata | Unsupported auth flows remain unavailable |

---

## Mandatory Reading

**CRITICAL: Implementation agent MUST read these files before starting any task:**

| Priority | File | Lines | Why Read This |
|----------|------|-------|---------------|
| P0 | `PRPs/prds/development-register-endpoint-access.prd.md` | all | Product scope, success metrics, and explicit non-goals |
| P0 | `PRPs/tasks/development-register-endpoint-access.md` | all | Source task and constraints |
| P0 | `server/FinancesControl.API/Common/IdentityEndpointRouteBuilderExtensions.cs` | 1-60 | Existing restricted Identity endpoint helper to update |
| P0 | `server/FinancesControl.API/Program.cs` | 59-100 | Identity registration, Development environment check, and endpoint mapping call site |
| P0 | `server/FinancesControl.Tests/Common/IdentityEndpointRouteBuilderExtensionsTests.cs` | 1-39 | Current allow-list unit test pattern and `/register` expectation |
| P0 | `server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs` | 1-140 | Hosted endpoint authorization tests and test-host environment setup |
| P1 | `server/FinancesControl.API/Common/IdentityOptionsConfiguration.cs` | 5-25 | Password policy needed for real register request bodies |
| P1 | `server/FinancesControl.API/Common/ProductionConfiguration.cs` | 14-20 | Existing environment-specific backend pattern |
| P1 | `server/FinancesControl.API/FinancesControl.API.csproj` | 8-11, 20-27 | InternalsVisibleTo and dependency versions |
| P1 | `server/FinancesControl.Tests/FinancesControl.Tests.csproj` | 10-18 | Test dependency versions |
| P2 | `PRPs/plans/completed/restrict-public-authentication-endpoints.plan.md` | all | Prior auth endpoint restriction context and rationale |
| P2 | `docs/dapper-usage-guidelines.md` | all | Project-required backend reading; no Dapper changes expected |

**External Documentation:**
| Source | Section | Why Needed |
|--------|---------|------------|
| [ASP.NET Core Identity API endpoints .NET 8](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-8.0#the-mapidentityapituser-endpoints) | The `MapIdentityApi<TUser>` endpoints | Confirms `POST /register` and the other generated endpoints in scope |
| [ASP.NET Core Identity API endpoints .NET 8](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-8.0#use-the-post-register-endpoint) | Use the `POST /register` endpoint | Confirms request body shape uses email and password |
| [ASP.NET Core Identity API endpoints .NET 8](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-8.0#map-identity-routes) | Map Identity routes | Confirms `MapIdentityApi<TUser>()` is the framework route mapping entry point |

---

## Patterns to Mirror

**NAMING_CONVENTION:**

```csharp
// SOURCE: server/FinancesControl.API/Common/IdentityEndpointRouteBuilderExtensions.cs:8-15
// COPY THIS PATTERN:
public static class IdentityEndpointRouteBuilderExtensions
{
    private const string LoginPath = "/login";
    private const string ManageInfoPath = "/manage/info";
    private const string RestrictedIdentityEndpointRole = "FinancesControl.UnsupportedIdentityEndpoint";

    public static IEndpointConventionBuilder MapRestrictedIdentityApi<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class, new()
```

**ENDPOINT_METADATA_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Common/IdentityEndpointRouteBuilderExtensions.cs:19-30
// COPY THIS PATTERN:
builder.Add(endpointBuilder =>
{
    if (endpointBuilder is not RouteEndpointBuilder routeEndpointBuilder)
        return;

    var routePattern = routeEndpointBuilder.RoutePattern.RawText;
    var httpMethods = endpointBuilder.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods;

    if (IsAllowedIdentityEndpoint(routePattern, httpMethods))
        return;

    endpointBuilder.Metadata.Add(new AuthorizeAttribute { Roles = RestrictedIdentityEndpointRole });
});
```

**ENVIRONMENT_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Program.cs:93-100
// COPY THIS PATTERN:
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapRestrictedIdentityApi<IdentityUser>()
    .RequireRateLimiting(RateLimitPolicyNames.Auth);
```

**CLASSIFIER_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Common/IdentityEndpointRouteBuilderExtensions.cs:36-44
// COPY THIS PATTERN:
internal static bool IsAllowedIdentityEndpoint(string? routePattern, IReadOnlyList<string>? httpMethods)
{
    if (string.IsNullOrWhiteSpace(routePattern) || httpMethods is null)
        return false;

    var normalizedRoutePattern = NormalizeRoutePattern(routePattern);

    return IsHttpMethod(httpMethods, HttpMethods.Post) && normalizedRoutePattern == LoginPath
        || IsHttpMethod(httpMethods, HttpMethods.Get) && normalizedRoutePattern == ManageInfoPath;
}
```

**TEST_STRUCTURE:**

```csharp
// SOURCE: server/FinancesControl.Tests/Common/IdentityEndpointRouteBuilderExtensionsTests.cs:8-19
// COPY THIS PATTERN:
[Theory]
[InlineData("/login", "POST")]
[InlineData("login", "post")]
[InlineData("/manage/info", "GET")]
[InlineData("manage/info", "get")]
public void IsAllowedIdentityEndpoint_WithAllowedEndpoint_ReturnsTrue(string routePattern, string httpMethod)
{
    var isAllowed = IdentityEndpointRouteBuilderExtensions.IsAllowedIdentityEndpoint(
        routePattern,
        [httpMethod]);

    isAllowed.Should().BeTrue();
}
```

**HOSTED_TEST_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs:107-121
// COPY THIS PATTERN:
private static WebApplicationFactory<Program> CreateFactory()
{
    var databaseName = $"FinancesControlIdentityTests-{Guid.NewGuid()}";

    return new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<FinancesDbContext>>();
                services.AddDbContext<FinancesDbContext>(options =>
                    options.UseInMemoryDatabase(databaseName));
            });
        });
}
```

---

## Files to Change

| File | Action | Justification |
| ---- | ------ | ------------- |
| `server/FinancesControl.API/Common/IdentityEndpointRouteBuilderExtensions.cs` | UPDATE | Add Development-only allow-list behavior for `POST /register` |
| `server/FinancesControl.API/Program.cs` | UPDATE | Pass the current Development environment state into the restricted Identity mapping helper |
| `server/FinancesControl.Tests/Common/IdentityEndpointRouteBuilderExtensionsTests.cs` | UPDATE | Cover `POST /register` as allowed only when Development is enabled |
| `server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs` | UPDATE | Cover Development registration reaching Identity and Testing registration remaining unauthorized |

---

## NOT Building (Scope Limits)

Explicit exclusions to prevent scope creep:

- No production public signup. `POST /register` must remain restricted outside Development.
- No custom registration controller. Continue using ASP.NET Core Identity's generated register endpoint.
- No password reset, email confirmation, refresh token, resend confirmation, 2FA, or profile mutation endpoint exposure.
- No frontend registration page or UI changes.
- No database schema, migration, Dapper repository, or domain service changes.
- No Swagger-only solution. Runtime endpoint behavior must be tested directly.

---

## Step-by-Step Tasks

Execute in order. Each task is atomic and independently verifiable.

**Status markers** - prefix EVERY task header with one; the build agent updates it inline as it works: `[ ]` idle . `[wip]` in progress . `[x]` complete . `[f]` failed. All tasks start `[ ]`. If a task cannot be made to pass, mark it `[f]`, record why in Agent Notes, and move on if the rest of the plan can still proceed.

### `[x]` Task 1: UPDATE `server/FinancesControl.API/Common/IdentityEndpointRouteBuilderExtensions.cs`

- **ACTION**: UPDATE the restricted Identity endpoint helper.
- **IMPLEMENT**: Add a `RegisterPath = "/register"` constant. Change `MapRestrictedIdentityApi<TUser>` to accept a boolean such as `allowDevelopmentRegistration`, or another clearly named environment flag passed by `Program.cs`. Pass that flag into `IsAllowedIdentityEndpoint`. Update `IsAllowedIdentityEndpoint` to accept the same flag and return true for `POST /register` only when the flag is true. Preserve `POST /login` and `GET /manage/info` behavior.
- **MIRROR**: `server/FinancesControl.API/Common/IdentityEndpointRouteBuilderExtensions.cs:10-12` for private constants; `server/FinancesControl.API/Common/IdentityEndpointRouteBuilderExtensions.cs:36-44` for the classifier shape.
- **IMPORTS**: No new imports expected.
- **GOTCHA**: Do not use `IHostEnvironment` directly inside this helper unless necessary; passing a boolean keeps the helper testable and avoids coupling the classifier to hosting infrastructure.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 2: UPDATE `server/FinancesControl.API/Program.cs`

- **ACTION**: UPDATE the Identity endpoint mapping call site.
- **IMPLEMENT**: Change `app.MapRestrictedIdentityApi<IdentityUser>()` to pass the Development state, for example `app.MapRestrictedIdentityApi<IdentityUser>(allowDevelopmentRegistration: app.Environment.IsDevelopment())`. Keep `.RequireRateLimiting(RateLimitPolicyNames.Auth)` chained exactly as it is.
- **MIRROR**: `server/FinancesControl.API/Program.cs:93-100` for existing Development branching and endpoint mapping style.
- **IMPORTS**: No new imports expected.
- **GOTCHA**: Do not broaden the condition to `Testing`; the PRD asks for Development only, and Testing should prove non-development remains restricted.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 3: UPDATE `server/FinancesControl.Tests/Common/IdentityEndpointRouteBuilderExtensionsTests.cs`

- **ACTION**: UPDATE classifier tests for environment-aware behavior.
- **IMPLEMENT**: Adjust calls to `IsAllowedIdentityEndpoint` to include the new boolean parameter. Keep existing allowed endpoint cases true regardless of flag. Move `/register` out of the always-restricted cases and add explicit tests that `POST /register` returns true when the Development flag is true and false when the flag is false. Keep `/refresh`, `/forgotPassword`, `/resetPassword`, `/confirmEmail`, `/resendConfirmationEmail`, `POST /manage/info`, and `POST /manage/2fa` false even when the flag is true.
- **MIRROR**: `server/FinancesControl.Tests/Common/IdentityEndpointRouteBuilderExtensionsTests.cs:8-19` for `[Theory]`, `[InlineData]`, and FluentAssertions style.
- **IMPORTS**: Existing imports should be enough: `using FinancesControl.API.Common;`, `using FluentAssertions;`.
- **GOTCHA**: Make the test names distinguish Development-only register behavior from the general allowed endpoint behavior, so future changes do not accidentally treat register as a normal public endpoint.
- **VALIDATE**: `dotnet test server/FinancesControl.sln --filter IdentityEndpointRouteBuilderExtensionsTests`

### `[x]` Task 4: UPDATE `server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs`

- **ACTION**: UPDATE hosted endpoint authorization tests.
- **IMPLEMENT**: Remove `/register` from the existing `RestrictedPostIdentityEndpoints_WithAnonymousRequest_ReturnUnauthorized` data set, or split the data into non-register restricted endpoints. Add a Testing environment test that anonymous `POST /register` still returns `Unauthorized`. Add a Development environment test that anonymous `POST /register` with a unique valid email and a password satisfying `IdentityOptionsConfiguration` reaches Identity registration and returns success. Generalize `CreateFactory` to accept an environment name while preserving the InMemory database override.
- **MIRROR**: `server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs:19-28` for anonymous endpoint reachability assertions; `server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs:107-121` for `WebApplicationFactory<Program>` setup.
- **IMPORTS**: Existing imports should be enough unless response body assertions are added.
- **GOTCHA**: The current `TestPassword` is `ValidPass123!`, which has 13 characters and satisfies the configured 12-character minimum plus digit, lowercase, uppercase, and non-alphanumeric requirements. Use unique emails for register success tests to avoid duplicate-user failures in shared test state.
- **VALIDATE**: `dotnet test server/FinancesControl.sln --filter IdentityEndpointAuthorizationTests`

---

## Testing Strategy

### Unit Tests to Write

| Test File | Test Cases | Validates |
| --------- | ---------- | --------- |
| `server/FinancesControl.Tests/Common/IdentityEndpointRouteBuilderExtensionsTests.cs` | Login and manage-info allowed regardless of flag; register allowed only when Development flag is true; all other hidden endpoints restricted regardless of flag | Pure endpoint allow-list rules |
| `server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs` | Development anonymous register succeeds; Testing anonymous register remains unauthorized; other hidden endpoints remain unauthorized | Runtime routing and authorization behavior |

### Edge Cases Checklist

- [ ] `POST /register` is allowed only when `app.Environment.IsDevelopment()` is true.
- [ ] `POST /register` remains unauthorized in `Testing`.
- [ ] `POST /register` with an invalid body in Development reaches Identity validation instead of endpoint authorization.
- [ ] `POST /login` remains reachable anonymously.
- [ ] `GET /manage/info` remains allowed by the helper but still requires authentication from Identity.
- [ ] `POST /refresh`, `POST /forgotPassword`, `POST /resetPassword`, `GET /confirmEmail`, `POST /resendConfirmationEmail`, `POST /manage/info`, and `POST /manage/2fa` remain restricted.
- [ ] `.RequireRateLimiting(RateLimitPolicyNames.Auth)` remains applied to the mapped Identity endpoint group.

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

**EXPECT**: Identity endpoint classifier and authorization tests pass.

### Level 3: FULL_SUITE

```bash
dotnet test server/FinancesControl.sln
```

**EXPECT**: All backend tests pass.

### Level 4: DATABASE_VALIDATION (if schema changes)

Not applicable. No schema changes or migrations should be made.

### Level 5: BROWSER_VALIDATION (if UI changes)

Not applicable. No frontend UI changes should be made.

### Level 6: MANUAL_VALIDATION

1. Run the API with `ASPNETCORE_ENVIRONMENT=Development`.
2. Open Swagger or call the API directly.
3. Execute `POST /register` with a unique email and a password that satisfies the configured policy.
4. Confirm the request is not rejected with `401` or `403`.
5. Log in with the new user using `POST /login?useCookies=true`.
6. Confirm authenticated frontend pages can be reached during local development.
7. Run or configure a non-Development environment and confirm anonymous `POST /register` remains unauthorized.

---

## Acceptance Criteria

- [ ] `POST /register` accepts anonymous requests in Development and reaches ASP.NET Core Identity registration behavior.
- [ ] `POST /register` remains restricted in Testing and non-Development environments.
- [ ] `POST /login` remains available to anonymous clients.
- [ ] `GET /manage/info` remains available for authenticated current-user checks and is not made anonymous.
- [ ] All other hidden Identity endpoints remain restricted everywhere.
- [ ] Existing rate limiting remains attached to the Identity endpoint mapping.
- [ ] Level 1-3 validation commands pass with exit 0.
- [ ] No database schema, frontend UI, or Dapper repository changes are introduced.

---

## Completion Checklist

- [ ] All tasks completed in dependency order.
- [ ] Each task validated immediately after completion.
- [ ] Level 1: Static analysis/build passes.
- [ ] Level 2: Identity endpoint tests pass.
- [ ] Level 3: Full backend test suite passes.
- [ ] Level 4: Database validation is confirmed not applicable.
- [ ] Level 5: Browser validation is confirmed not applicable.
- [ ] All acceptance criteria met.

---

## Risks and Mitigations

| Risk | Likelihood | Impact | Mitigation |
| ---- | ---------- | ------ | ---------- |
| Register accidentally becomes public outside Development | MED | HIGH | Pass only `app.Environment.IsDevelopment()` and test Testing/non-Development behavior |
| Other hidden endpoints become allowed through overly broad logic | MED | HIGH | Keep endpoint-specific route and method checks; test all hidden endpoints with the flag true |
| Hosted Development test is affected by HTTPS redirection | LOW | LOW | `WebApplicationFactory` follows redirects by default; if needed, assert only final status is not `401`/`403` or configure client options |
| Registration success test fails due password policy | LOW | MED | Use a password satisfying `IdentityOptionsConfiguration` and unique email per test |
| Swagger visibility is mistaken for runtime behavior | MED | MED | Runtime authorization tests are required; Swagger visibility is optional only |

---

## Questionables

<details>
<summary>Should Testing allow register for integration setup?</summary>

Assumption: No. The PRD says Development only, and existing tests already use service-level seeding through `UserManager<IdentityUser>` when a test user is needed.

</details>

<details>
<summary>Should the plan require Swagger visibility tests?</summary>

Assumption: No. The success condition is runtime access. Swagger already runs only in Development, and OpenAPI visibility can be manually checked after runtime behavior is correct.

</details>

---

## Agent Notes

### Discovery Summary

The current implementation already has a small, testable Identity endpoint wrapper. `MapRestrictedIdentityApi<TUser>()` maps all generated Identity endpoints, inspects route pattern and HTTP method metadata, and adds an impossible role requirement to unsupported endpoints. The new change should be a surgical extension of that pattern.

### External Research Notes

Microsoft's ASP.NET Core Identity API documentation for .NET 8 lists `POST /register`, `POST /login`, `POST /refresh`, `GET /confirmEmail`, `POST /resendConfirmationEmail`, `POST /forgotPassword`, `POST /resetPassword`, `POST /manage/2fa`, `GET /manage/info`, and `POST /manage/info` as endpoints generated by `MapIdentityApi<TUser>()`. The same documentation states the register request body uses `email` and `password`, matching the planned Development test.

### Approach Chosen

APPROACH_CHOSEN: Add an environment-aware exception to the existing allow-list so `POST /register` is public only when `Program.cs` passes `app.Environment.IsDevelopment()`.

RATIONALE: This preserves the security posture from the prior Identity restriction plan, keeps the logic in the existing helper, and avoids inventing a parallel registration endpoint.

ALTERNATIVES_REJECTED:

- Custom registration controller: rejected because ASP.NET Core Identity already supplies the desired development behavior and the request is not a product registration feature.
- Allow register in Testing too: rejected because the user asked for Development and tests can seed users through `UserManager`.
- Swagger-only filtering: rejected because visibility is not runtime access control.
- Reopen all Identity endpoints in Development: rejected because only register is needed for test-user creation.

---

## Amendments

<details>
<summary>2026-07-19T16:35:00-03:00 - Initial plan</summary>

Created the implementation plan from `PRPs/tasks/development-register-endpoint-access.md` and linked it to the source PRD.

</details>

<details>
<summary>2026-07-19T16:55:00-03:00 - Implementation complete</summary>

Added a Development-only `POST /register` exception to the existing restricted Identity endpoint helper, passed `app.Environment.IsDevelopment()` from startup, updated classifier tests, and added hosted authorization coverage for Development and Testing behavior. Validation passed with build, Identity endpoint tests, and full backend suite.

</details>
