# Feature: Suppress HTTPS Redirection Noise In Backend Tests

## Summary

Suppress repeated ASP.NET Core HTTPS redirection warnings during backend integration tests by using the existing `Testing` environment pattern and skipping `app.UseHttpsRedirection()` only when the API runs under that environment. Add a focused regression test around the `WebApplicationFactory<Program>` test host so the behavior remains intentional and production HTTPS redirection stays unchanged.

## User Story

As a backend developer
I want backend test output to omit expected HTTPS redirection warnings
So that meaningful warnings and failures are easier to notice during test runs

## Problem Statement

Backend integration tests build the real API pipeline through `WebApplicationFactory<Program>`, but the test host does not expose a real HTTPS endpoint or configured HTTPS port. Because `Program.cs` calls `app.UseHttpsRedirection()` unconditionally, requests made by test clients can trigger `HttpsRedirectionMiddleware` warnings such as "Failed to determine the https port for redirect", creating noisy test output without indicating a product failure.

## Solution Statement

Guard `app.UseHttpsRedirection()` in `Program.cs` with `!app.Environment.IsEnvironment("Testing")`, mirroring the existing environment-specific Swagger branch in the same startup file. Keep the existing `builder.UseEnvironment("Testing")` test host pattern, add a regression test in `server/FinancesControl.Tests/Common`, and leave EF Core InMemory save logging untouched.

## Metadata

| Field            | Value |
| ---------------- | ----- |
| Type             | BUG_FIX |
| Complexity       | LOW |
| Systems Affected | Backend API startup pipeline, backend integration test host |
| Dependencies     | Microsoft.AspNetCore.Mvc.Testing 8.0.23, Microsoft.EntityFrameworkCore.InMemory 8.0.23, xUnit 2.9.2, FluentAssertions 4.19.4 |
| Estimated Tasks  | 2 |

---

## Lifecycle (append-only)

- **Created:** 2026-07-19T04:23:12.1238822-03:00
- **Modified:**
  - 2026-07-19T04:23:12.1238822-03:00 - Initial plan from task document.
  - 2026-07-19T04:33:57.0732770-03:00 - Implemented Testing-only HTTPS redirection guard and regression test.
- **Commits:** none yet
- **Agent / Session:**
  - Codex GPT-5 session - initial planning
  - Codex GPT-5 session - implementation
- **Back refs:**
  - `PRPs/tasks/suppress-https-redirection-noise-in-backend-tests.md` - source task
- **Forward refs:** none yet

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
|   | Backend     | ------> | HTTP test   | ------> | Noisy test  |             |
|   | test run    |         | request     |         | output      |             |
|   +-------------+         +-------------+         +-------------+             |
|                                                                              |
|   USER_FLOW: Developer runs dotnet test for backend tests.                    |
|   PAIN_POINT: Expected HTTPS redirection warnings repeat in test output.      |
|   DATA_FLOW: WebApplicationFactory -> Program.cs -> UseHttpsRedirection       |
|              -> TestServer without HTTPS port -> middleware warning.          |
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
|   | Backend     | ------> | Testing env | ------> | Quiet test  |             |
|   | test run    |         | pipeline    |         | output      |             |
|   +-------------+         +-------------+         +-------------+             |
|                                  |                                           |
|                                  v                                           |
|                          +---------------+                                   |
|                          | HTTPS skipped | <-- Testing-only middleware gate   |
|                          +---------------+                                   |
|                                                                              |
|   USER_FLOW: Developer runs dotnet test and sees only meaningful output.      |
|   VALUE_ADD: HTTPS redirect noise no longer hides real warnings.              |
|   DATA_FLOW: WebApplicationFactory -> UseEnvironment("Testing")              |
|              -> Program.cs guard -> no HttpsRedirectionMiddleware warning.    |
|                                                                              |
+------------------------------------------------------------------------------+
```

### Interaction Changes

| Location | Before | After | User Impact |
|----------|--------|-------|-------------|
| `Program.cs` test-host pipeline | `UseHttpsRedirection()` runs in all environments | `UseHttpsRedirection()` runs except in `Testing` | Test output is quieter while non-test environments keep existing HTTPS behavior |
| `server/FinancesControl.Tests/Common` | Existing integration tests use `Testing` but do not assert the HTTPS-noise contract | A focused regression test documents the `Testing` environment expectation | Future middleware changes are less likely to reintroduce warning noise |

---

## Mandatory Reading

**CRITICAL: Implementation agent MUST read these files before starting any task:**

| Priority | File | Lines | Why Read This |
|----------|------|-------|---------------|
| P0 | `server/FinancesControl.API/Program.cs` | 91-113 | Middleware ordering and exact `UseHttpsRedirection()` placement to update |
| P0 | `server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs` | 107-121 | Existing `WebApplicationFactory<Program>` and `UseEnvironment("Testing")` pattern to mirror |
| P1 | `docs/backend-testing-guidelines.md` | all | xUnit, FluentAssertions, naming, and validation conventions; note target framework in docs is stale versus csproj |
| P1 | `server/FinancesControl.Tests/FinancesControl.Tests.csproj` | 1-23 | Actual test package versions and `net8.0` target framework |
| P2 | `PRPs/tasks/suppress-https-redirection-noise-in-backend-tests.md` | all | Source constraints, especially leaving EF Core InMemory save logs alone |

**External Documentation:**
| Source | Section | Why Needed |
|--------|---------|------------|
| [ASP.NET Core integration tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0#set-the-environment) | Set the environment | Confirms `WebApplicationFactory` supports `builder.UseEnvironment(...)` for test-specific environments |
| [ASP.NET Core integration tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0#inject-mock-services) | HTTPS redirection warning note | Documents the warning source and HTTPS-base-address alternative |
| [Enforce HTTPS in ASP.NET Core 8](https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-8.0#port-configuration) | Port configuration | Confirms middleware logs the warning when no HTTPS port is available |
| [ASP.NET Core environments](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-8.0#environment-variables-that-determine-the-runtime-environment) | Runtime environment names | Confirms custom environment names such as `Testing` are valid and checked at startup |

---

## Patterns to Mirror

**NAMING_CONVENTION:**

```csharp
// SOURCE: server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs:14-20
// COPY THIS PATTERN:
public class IdentityEndpointAuthorizationTests
{
    private const string TestEmail = "identity-endpoints@test.local";
    private const string TestPassword = "ValidPass123!";

    [Fact]
    public async Task Login_WithAnonymousRequest_ReachesIdentityLoginEndpoint()
```

**ERROR_HANDLING:**

```csharp
// SOURCE: server/FinancesControl.API/Common/Middleware/GlobalExceptionHandlerMiddleware.cs:18-25
// COPY THIS PATTERN:
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    catch (Exception ex)
```

This task should not add new error handling; the snippet is included so implementation agents preserve the current middleware chain and do not replace exception behavior.

**LOGGING_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Common/Middleware/GlobalExceptionHandlerMiddleware.cs:41-44
// COPY THIS PATTERN:
if (shouldLogAsError)
    _logger.LogError(exception, "Unhandled exception while processing the request.");
else
    _logger.LogInformation(exception, "Handled application exception while processing the request.");
```

This task should not change logging configuration or suppress EF Core InMemory logs; the desired behavior comes from not adding HTTPS redirection middleware in `Testing`.

**REPOSITORY_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Program.cs:55-60
// COPY THIS PATTERN:
builder.Services.AddDbContext<FinancesDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Database connection factory for Dapper
builder.Services.AddScoped<AppDbConnectionFactory>();
```

No repository changes are required. This snippet documents the default DB registration that existing integration tests replace.

**SERVICE_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Program.cs:91-105
// COPY THIS PATTERN:
var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapRestrictedIdentityApi<IdentityUser>()
    .RequireRateLimiting(RateLimitPolicyNames.Auth);

app.UseHttpsRedirection();
```

Use a compact environment guard in this same middleware area.

**TEST_STRUCTURE:**

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
| `server/FinancesControl.API/Program.cs` | UPDATE | Skip HTTPS redirection only when the host environment is `Testing` |
| `server/FinancesControl.Tests/Common/HttpsRedirectionTestingEnvironmentTests.cs` | CREATE | Regression test proving the `Testing` host avoids HTTPS redirection noise while using existing test-host patterns |

---

## NOT Building (Scope Limits)

Explicit exclusions to prevent scope creep:

- Do not suppress or filter EF Core InMemory save logs; the source task explicitly leaves those alone.
- Do not introduce shared test infrastructure unless the implementation naturally needs it; existing integration tests use local `CreateFactory()` helpers.
- Do not change production, development, staging, or default environment HTTPS behavior.
- Do not add or change database scripts, repositories, services, controllers, frontend code, or documentation files.

---

## Step-by-Step Tasks

Execute in order. Each task is atomic and independently verifiable.

**Status markers** - prefix EVERY task header with one; the build agent updates it inline as it works: `[ ]` idle . `[wip]` in progress . `[x]` complete . `[f]` failed. All tasks start `[ ]`. If a task cannot be made to pass, mark it `[f]`, record why in Agent Notes, and move on if the rest of the plan can still proceed.

### `[x]` Task 1: UPDATE `server/FinancesControl.API/Program.cs`

- **ACTION**: UPDATE middleware registration.
- **IMPLEMENT**: Replace the unconditional `app.UseHttpsRedirection();` call with an environment guard:

  ```csharp
  if (!app.Environment.IsEnvironment("Testing"))
  {
      app.UseHttpsRedirection();
  }
  ```

- **MIRROR**: `server/FinancesControl.API/Program.cs:95-100` - follow the existing compact environment-specific middleware branch used for Swagger.
- **IMPORTS**: No new imports required; `WebApplication` environment exposes `IsEnvironment`.
- **GOTCHA**: Keep the guard narrowly scoped to `Testing`. Do not use `IsDevelopment()` or remove HTTPS redirection globally, because non-test environments must retain existing behavior.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 2: CREATE `server/FinancesControl.Tests/Common/HttpsRedirectionTestingEnvironmentTests.cs`

- **ACTION**: CREATE a focused regression test.
- **IMPLEMENT**: Add a test class named `HttpsRedirectionTestingEnvironmentTests` with a `[Fact]` named `TestingEnvironment_WithHttpRequest_DoesNotLogHttpsPortWarning`. Use `WebApplicationFactory<Program>().WithWebHostBuilder(...)`, set `builder.UseEnvironment("Testing")`, replace `FinancesDbContext` with EF Core InMemory using a unique database name, configure test logging to capture warning messages, issue a simple HTTP request with `factory.CreateClient()`, and assert captured warning messages do not contain `Failed to determine the https port for redirect`.
- **MIRROR**: `server/FinancesControl.Tests/Common/IdentityEndpointAuthorizationTests.cs:107-121` - mirror the local factory setup using `UseEnvironment("Testing")`, `RemoveAll<DbContextOptions<FinancesDbContext>>()`, and `UseInMemoryDatabase(databaseName)`.
- **PATTERN**: Follow `docs/backend-testing-guidelines.md`: class name ends with `Tests`, method name follows `MethodOrScenario_WithCondition_ExpectedResult`, use xUnit `[Fact]`, and use FluentAssertions for assertions.
- **IMPORTS**: Expected imports include `FinancesControl.API.Data`, `FluentAssertions`, `Microsoft.AspNetCore.Hosting`, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.DependencyInjection.Extensions`, and `Microsoft.Extensions.Logging`.
- **GOTCHA**: The test must prove HTTPS redirection warning noise is gone without filtering all warnings. Do not assert an exact response status unless needed; the regression target is the absence of the HTTPS-port warning under `Testing`.
- **VALIDATE**: `dotnet test server/FinancesControl.sln --filter FullyQualifiedName~HttpsRedirectionTestingEnvironmentTests`

---

## Testing Strategy

### Unit Tests to Write

| Test File | Test Cases | Validates |
| --------- | ---------- | --------- |
| `server/FinancesControl.Tests/Common/HttpsRedirectionTestingEnvironmentTests.cs` | `TestingEnvironment_WithHttpRequest_DoesNotLogHttpsPortWarning` | `Testing` environment does not register HTTPS redirection middleware noise in integration-test host |

### Edge Cases Checklist

- [ ] Test host explicitly sets `Testing` before the app pipeline is built.
- [ ] Test uses default HTTP `CreateClient()` behavior, because that is the scenario that currently triggers the warning.
- [ ] Test captures warning text without suppressing all test logs.
- [ ] Production/default environment behavior remains unchanged because the guard excludes only `Testing`.
- [ ] EF Core InMemory save logs remain untouched.

---

## Validation Commands

**IMPORTANT**: Replace these placeholders with actual commands from the project's package.json/config.

Validation loop: the plan is not complete until every command below passes (exit 0). On any failure, fix the cause and re-run - loop until all pass. If a check is genuinely impossible, mark it `[f]`, note why in Agent Notes, and move on.

### Level 1: STATIC_ANALYSIS

```bash
dotnet build server/FinancesControl.sln
```

**EXPECT**: Exit 0 with no compiler errors.

### Level 2: UNIT_TESTS

```bash
dotnet test server/FinancesControl.sln --filter FullyQualifiedName~HttpsRedirectionTestingEnvironmentTests
```

**EXPECT**: New regression test passes and fails before the `Program.cs` guard if the warning is captured.

### Level 3: FULL_SUITE

```bash
dotnet test server/FinancesControl.sln
```

**EXPECT**: All backend tests pass.

### Level 4: DATABASE_VALIDATION (if schema changes)

Not applicable. No database schema or SQL changes.

### Level 5: BROWSER_VALIDATION (if UI changes)

Not applicable. No frontend or browser behavior changes.

### Level 6: MANUAL_VALIDATION

Run `dotnet test server/FinancesControl.sln` and confirm the previous HTTPS redirection warning text no longer appears in backend test output. Confirm any EF Core InMemory save logs are not part of this task and were not intentionally suppressed.

---

## Acceptance Criteria

- [ ] `Program.cs` skips `app.UseHttpsRedirection()` only when `app.Environment.IsEnvironment("Testing")` is true.
- [ ] Non-test environments retain existing HTTPS redirection middleware behavior.
- [ ] A regression test covers the `Testing` WebApplicationFactory host and default HTTP client path.
- [ ] The regression test asserts the HTTPS-port warning is absent without globally silencing unrelated logs.
- [ ] `dotnet build server/FinancesControl.sln` passes.
- [ ] `dotnet test server/FinancesControl.sln` passes.
- [ ] EF Core InMemory save logs are left unchanged.

---

## Completion Checklist

- [ ] All tasks completed in dependency order
- [ ] Each task validated immediately after completion
- [ ] Level 1: Static analysis (build) passes
- [ ] Level 2: Focused regression test passes
- [ ] Level 3: Full backend test suite passes
- [ ] Level 4: Database validation skipped as not applicable
- [ ] Level 5: Browser validation skipped as not applicable
- [ ] All acceptance criteria met

---

## Risks and Mitigations

| Risk | Likelihood | Impact | Mitigation |
| ---- | ---------- | ------ | ---------- |
| Accidentally disabling HTTPS redirection outside tests | LOW | HIGH | Use `!app.Environment.IsEnvironment("Testing")` only, and do not remove the middleware call |
| Regression test becomes brittle if it asserts exact endpoint response behavior | MED | LOW | Keep the assertion focused on captured warning messages, not unrelated endpoint details |
| Logging capture test misses the middleware category | MED | MED | Capture warning messages from configured logging provider broadly enough to inspect message text |
| Docs mention `net9.0` while projects target `net8.0` | MED | LOW | Follow `*.csproj` reality for commands and dependency versions; do not edit docs in this task |

---

## Questionables

<details>
<summary>Should tests instead use HTTPS BaseAddress?</summary>

Assumption: implement the `Testing` environment guard rather than changing every test client to `BaseAddress = new Uri("https://localhost")`. Microsoft documents HTTPS base address as a valid warning-avoidance technique, but the source task prefers a dedicated testing environment and this repo already uses `builder.UseEnvironment("Testing")`.

</details>

<details>
<summary>Should a shared WebApplicationFactory helper be introduced?</summary>

Assumption: do not introduce shared infrastructure for this small fix. Existing integration tests use a private `CreateFactory()` helper inside `IdentityEndpointAuthorizationTests`, and this task does not require broader integration-test cleanup.

</details>

---

## Agent Notes

APPROACH_CHOSEN: Environment-gate HTTPS redirection in `Program.cs` for `Testing`, plus a focused regression test.

RATIONALE: The API already uses environment-specific middleware branching for Swagger in `Program.cs:95-100`, and integration tests already configure `builder.UseEnvironment("Testing")` in `IdentityEndpointAuthorizationTests.cs:112-115`. This keeps production behavior unchanged and removes warning noise at the source.

ALTERNATIVES_REJECTED:

- Configure `HttpsRedirection:HttpsPort` in test configuration: rejected because tests do not need redirect behavior and the source task explicitly allows skipping the middleware under `Testing`.
- Set `WebApplicationFactoryClientOptions.BaseAddress = new Uri("https://localhost")`: rejected as the main approach because it requires changing client creation patterns and does not document that HTTPS redirection is intentionally absent in `Testing`.
- Suppress logger categories in tests: rejected because it could hide meaningful warnings and violates the goal of removing only expected noise.

External research:

- Microsoft integration-test docs note that `CreateClient()` can produce HTTPS redirection warnings and suggest HTTPS `BaseAddress` as one mitigation.
- Microsoft HTTPS docs confirm the middleware logs the failed HTTPS-port warning when no HTTPS port is available.
- Microsoft environment docs confirm custom environment names can be checked with `IsEnvironment("Testing")`.

No database, frontend, routing, repository, or documentation changes are part of this plan.

---

## Amendments

<details>
<summary>2026-07-19T04:23:12.1238822-03:00 - Initial plan</summary>

Created implementation plan from `PRPs/tasks/suppress-https-redirection-noise-in-backend-tests.md` after codebase exploration and external documentation review.

</details>

<details>
<summary>2026-07-19T04:33:57.0732770-03:00 - Implementation complete</summary>

Implemented the `Testing` environment guard around `app.UseHttpsRedirection()` and added `HttpsRedirectionTestingEnvironmentTests` to capture warnings from a `WebApplicationFactory<Program>` host. Validation passed with `dotnet build server/FinancesControl.sln`, focused regression test filtering, and the full backend test suite.

</details>
