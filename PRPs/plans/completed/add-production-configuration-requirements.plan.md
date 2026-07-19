# Feature: Add Production Configuration Requirements

## Summary

Add explicit production configuration requirements for the backend API so production-like environments fail fast when `ConnectionStrings:DefaultConnection`, `Cors:AllowedOrigins`, or `AllowedHosts` are missing or permissive. The implementation should centralize validation in a small `Common` helper, reuse the validated CORS origins and connection string in `Program.cs`, replace committed wildcard host config with deployment placeholders, and document the required environment variables without committing secrets.

## User Story

As a finance app operator
I want required production configuration to be explicit and validated at startup
So that the API does not run in production with missing database settings or broad host/CORS exposure

## Problem Statement

`server/FinancesControl.API/appsettings.json` currently defines only logging and `"AllowedHosts": "*"`, while `DefaultConnection` and `Cors:AllowedOrigins` exist only in development config. `Program.cs` silently falls back to an empty CORS origin array and passes a possibly missing connection string to Npgsql. This is testable by starting/configuring the app with missing production settings and asserting startup validation fails before the API serves requests.

## Solution Statement

Create a production configuration model/helper in `server/FinancesControl.API/Common` that reads `ConnectionStrings:DefaultConnection`, `Cors:AllowedOrigins`, and `AllowedHosts`, validates them for non-Development/non-Testing environments, and returns normalized values for startup. Wire `Program.cs` to call the helper once after `WebApplication.CreateBuilder(args)`, use the returned connection string for EF Core, and use the returned origins for the named CORS policy. Update base `appsettings.json` to declare non-secret placeholder keys instead of wildcard hosts, preserve local values in `appsettings.Development.json`, and add backend tests for valid production config, missing values, wildcard rejection, and Development/Testing bypass behavior.

## Metadata

| Field            | Value |
| ---------------- | ----- |
| Type             | ENHANCEMENT |
| Complexity       | MEDIUM |
| Systems Affected | Backend API startup, ASP.NET Core configuration, CORS, EF Core/Npgsql registration, backend tests, deployment docs |
| Dependencies     | .NET 8 / ASP.NET Core configuration and CORS, Npgsql.EntityFrameworkCore.PostgreSQL 8.0.11, Microsoft.AspNetCore.Mvc.Testing 8.0.23, Microsoft.EntityFrameworkCore.InMemory 8.0.23, xUnit 2.9.2, FluentAssertions 4.19.4 |
| Estimated Tasks  | 5 |

---

## Lifecycle (append-only)

- **Created:** 2026-07-19T00:00:00-03:00
- **Modified:** 2026-07-19T00:00:00-03:00 - Initial plan created from `PRPs/tasks/add-production-configuration-requirements.md`
- **Modified:** 2026-07-19T05:38:15-03:00 - Implemented production configuration validation, startup wiring, tests, and documentation
- **Commits:** None yet
- **Commits:** Not committed in this session
- **Agent / Session:** Codex GPT-5 planning session, 2026-07-19
- **Agent / Session:** Codex GPT-5 implementation session, 2026-07-19
- **Back refs:** `PRPs/tasks/add-production-configuration-requirements.md` - source task; `docs/features/backend-production-readiness-fixes.md` - production-readiness checklist item
- **Forward refs:** None yet

> **Append-only:** `Created` is set once; every other field is a list you only ever add to - never overwrite or remove existing entries. Keep references bidirectional: when you add a back/forward ref here, add the reciprocal ref on the other plan.

---

## UX Design

### Before State

```
+------------------------------------------------------------------------------+
|                                 BEFORE STATE                                  |
+------------------------------------------------------------------------------+
|                                                                              |
|   Production host ---> Program.cs reads config ---> API may start with gaps   |
|                                                                              |
|   USER_FLOW: Operator deploys the backend and relies on runtime config from   |
|   appsettings files or environment variables.                                 |
|   PAIN_POINT: Missing DefaultConnection can fail later during DB use, missing |
|   Cors:AllowedOrigins silently becomes empty, and AllowedHosts "*" permits    |
|   every non-empty Host header.                                                |
|   DATA_FLOW: builder.Configuration -> inline CORS array / UseNpgsql string -> |
|   app pipeline and database registrations.                                    |
|                                                                              |
+------------------------------------------------------------------------------+
```

### After State

```
+------------------------------------------------------------------------------+
|                                  AFTER STATE                                  |
+------------------------------------------------------------------------------+
|                                                                              |
|   Production host ---> ProductionConfiguration.Validate ---> explicit startup |
|          |                                    |                               |
|          v                                    v                               |
|   Valid DB/CORS/hosts ------------------> API starts safely                   |
|   Missing/wildcard values ---------------> startup fails with clear message   |
|                                                                              |
|   USER_FLOW: Operator deploys with required environment variables or concrete |
|   deployment configuration before the API can serve requests.                 |
|   VALUE_ADD: Misconfigured production deployments fail early and reviewably,  |
|   reducing secret leakage and broad host/CORS exposure.                       |
|   DATA_FLOW: builder.Configuration + environment -> validated values -> CORS  |
|   policy and EF Core registration.                                            |
|                                                                              |
+------------------------------------------------------------------------------+
```

### Interaction Changes

| Location | Before | After | User Impact |
|----------|--------|-------|-------------|
| `Program.cs` startup | Reads `Cors:AllowedOrigins` inline and accepts missing values | Calls a validation helper and uses normalized values | Operators get clear startup failures instead of hidden misconfiguration |
| `appsettings.json` | Contains `"AllowedHosts": "*"` and no required config shape | Contains non-secret placeholders/comments-free JSON keys for hosts, CORS, and connection string | Deployment reviewers can see every required setting without secrets |
| Production deployment config | Required variables are not documented in repo | Required environment variables are documented | Deployments have a concrete checklist |

---

## Mandatory Reading

**CRITICAL: Implementation agent MUST read these files before starting any task:**

| Priority | File | Lines | Why Read This |
|----------|------|-------|---------------|
| P0 | `PRPs/tasks/add-production-configuration-requirements.md` | all | Source requirement and expected direction |
| P0 | `server/FinancesControl.API/Program.cs` | 19-40, 55-68, 91-110 | Existing startup composition, CORS policy, EF Core connection-string usage, and environment branching |
| P0 | `server/FinancesControl.API/appsettings.json` | 1-8 | Current committed wildcard `AllowedHosts` problem |
| P0 | `server/FinancesControl.API/appsettings.Development.json` | 1-16 | Current local-only connection string and CORS shape to preserve for development |
| P1 | `server/FinancesControl.API/Common/IdentityOptionsConfiguration.cs` | 3-26 | Static `Common` configuration helper style to mirror |
| P1 | `server/FinancesControl.API/Data/AppDbConnectionFactory.cs` | 7-17 | Dapper connection factory still reads `DefaultConnection` from configuration |
| P1 | `server/FinancesControl.Tests/Common/HttpsRedirectionTestingEnvironmentTests.cs` | 13-52 | Existing `WebApplicationFactory<Program>` and `UseEnvironment("Testing")` pattern |
| P1 | `docs/backend-testing-guidelines.md` | 71-114 | Backend test requirements and validation command |
| P2 | `docs/dapper-usage-guidelines.md` | 43-58 | Confirms the documented shared connection factory key |
| P2 | `docs/features/backend-production-readiness-fixes.md` | 43-51 | Parent production-readiness checklist item |

**External Documentation:**
| Source | Section | Why Needed |
|--------|---------|------------|
| [ASP.NET Core Configuration .NET 8](https://learn.microsoft.com/en-ca/aspnet/core/fundamentals/configuration/?view=aspnetcore-8.0#configuration-keys-and-values) | Configuration keys and values | Confirms hierarchical config keys and environment-variable behavior |
| [Options pattern .NET](https://learn.microsoft.com/en-ca/dotnet/core/extensions/options#options-validation) | Options validation / `ValidateOnStart` | Confirms startup-time validation is supported when using options |
| [ASP.NET Core CORS .NET 8](https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-8.0#set-the-allowed-origins) | Set the allowed origins | Confirms explicit origins and warns that `AllowAnyOrigin` is insecure |
| [ASP.NET Core CORS .NET 8](https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-8.0#credentials-in-cross-origin-requests) | Credentials in cross-origin requests | Confirms wildcard origins are invalid/risky with credentials |
| [HostFilteringOptions.AllowedHosts](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.hostfiltering.hostfilteringoptions.allowedhosts?view=aspnetcore-8.0) | Remarks | Confirms `*` allows all non-empty hosts and ports must be excluded |

---

## Patterns to Mirror

**NAMING_CONVENTION:**

```csharp
// SOURCE: server/FinancesControl.API/Common/IdentityOptionsConfiguration.cs:3-8
// COPY THIS PATTERN:
namespace FinancesControl.API.Common;

public static class IdentityOptionsConfiguration
{
    public static void Configure(IdentityOptions options)
```

**ERROR_HANDLING:**

```csharp
// SOURCE: server/FinancesControl.API/Common/IdentityOptionsConfiguration.cs:7-25
// COPY THIS PATTERN:
public static void Configure(IdentityOptions options)
{
    options.Password.RequiredLength = 12;
    options.Password.RequiredUniqueChars = 4;
```

Use direct, deterministic helper validation. For invalid production configuration, throw `InvalidOperationException` with a message naming the missing or unsafe config key; no domain exception is needed because this is startup infrastructure, not request/domain validation.

**LOGGING_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Common/Middleware/GlobalExceptionHandlerMiddleware.cs:41-44
// COPY THIS PATTERN:
if (shouldLogAsError)
    _logger.LogError(exception, "Unhandled exception while processing the request.");
else
    _logger.LogInformation(exception, "Handled application exception while processing the request.");
```

No new logging is required for fail-fast startup validation. If logging is added, keep messages clear and avoid printing the connection string or secrets.

**REPOSITORY_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Data/AppDbConnectionFactory.cs:16-17
// COPY THIS PATTERN:
public DbConnection CreateConnection()
    => new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
```

No repository changes are planned. Keep the same `ConnectionStrings:DefaultConnection` key so EF Core and Dapper continue to share configuration.

**SERVICE_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Program.cs:26-39
// COPY THIS PATTERN:
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
```

Move the config read out of the lambda, but preserve the named policy and `WithOrigins(...).AllowAnyHeader().AllowAnyMethod().AllowCredentials()` behavior with validated origins.

**TEST_STRUCTURE:**

```csharp
// SOURCE: server/FinancesControl.Tests/Common/HttpsRedirectionTestingEnvironmentTests.cs:17-30
// COPY THIS PATTERN:
[Fact]
public async Task TestingEnvironment_WithHttpRequest_DoesNotLogHttpsPortWarning()
{
    var warningMessages = new ConcurrentQueue<string>();
    using var loggerProvider = new WarningCaptureLoggerProvider(warningMessages);
    using var factory = CreateFactory(loggerProvider);
    using var client = factory.CreateClient();

    _ = await client.GetAsync("/manage/info");

    warningMessages
        .Any(message => message.Contains(HttpsPortWarning, StringComparison.OrdinalIgnoreCase))
        .Should()
        .BeFalse();
}
```

Use xUnit `[Fact]`/`[Theory]`, behavior-focused names, and FluentAssertions in `server/FinancesControl.Tests/Common`.

---

## Files to Change

| File | Action | Justification |
| ---- | ------ | ------------- |
| `server/FinancesControl.API/Common/ProductionConfiguration.cs` | CREATE | Centralizes production config parsing/validation and normalized values |
| `server/FinancesControl.API/Program.cs` | UPDATE | Validates config once and reuses validated DB/CORS values in startup |
| `server/FinancesControl.API/appsettings.json` | UPDATE | Replaces wildcard host and declares required non-secret config shape |
| `server/FinancesControl.Tests/Common/ProductionConfigurationTests.cs` | CREATE | Covers fail-fast and environment bypass behavior without needing a real database |
| `docs/features/backend-production-readiness-fixes.md` or a new deployment config note under `docs/features/` | UPDATE | Documents required environment variables/deployment keys without secrets |

---

## NOT Building (Scope Limits)

Explicit exclusions to prevent scope creep:

- Do not add real production secrets, passwords, certificates, or deployment-provider credentials to source control.
- Do not change database schema, migrations, Dapper repositories, or EF Core models.
- Do not introduce a new secrets manager or cloud-specific deployment system; document required keys in provider-neutral terms.
- Do not change CORS methods/headers/credentials behavior except validating origins before use.
- Do not remove the existing `Development` connection string as part of this task, unless the implementation agent separately confirms it is already rotated and safe to move to user secrets.

---

## Step-by-Step Tasks

Execute in order. Each task is atomic and independently verifiable.

**Status markers** - prefix EVERY task header with one; the build agent updates it inline as it works: `[ ]` idle . `[wip]` in progress . `[x]` complete . `[f]` failed. All tasks start `[ ]`. If a task cannot be made to pass, mark it `[f]`, record why in Agent Notes, and move on if the rest of the plan can still proceed.

### `[x]` Task 1: CREATE `server/FinancesControl.API/Common/ProductionConfiguration.cs`

- **ACTION**: CREATE a static startup configuration helper and immutable result type.
- **IMPLEMENT**: Add `namespace FinancesControl.API.Common;`. Create `public sealed record ProductionConfiguration(string DefaultConnection, string[] CorsAllowedOrigins);` and `public static class ProductionConfigurationValidator` with `public static ProductionConfiguration Validate(IConfiguration configuration, IHostEnvironment environment)`. In `Development` and `Testing`, allow current local behavior but still return `DefaultConnection` and CORS origins from configuration using empty-string/empty-array fallbacks. In all other environments, require non-empty `ConnectionStrings:DefaultConnection`, require at least one non-empty `Cors:AllowedOrigins` value, require non-empty `AllowedHosts`, and reject `AllowedHosts` values that are exactly `"*"`. Reject CORS origins that are `"*"`, whitespace, or end with `/`.
- **MIRROR**: `server/FinancesControl.API/Common/IdentityOptionsConfiguration.cs:3-8` - static helper in `FinancesControl.API.Common`.
- **IMPORTS**: `using Microsoft.Extensions.Configuration;`, `using Microsoft.Extensions.Hosting;`.
- **GOTCHA**: Microsoft CORS docs note origin URLs must not contain a trailing slash and wildcard origins are unsafe/invalid with credentials. Microsoft HostFiltering docs state `AllowedHosts` port numbers must be excluded and `*` allows all non-empty hosts.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 2: UPDATE `server/FinancesControl.API/Program.cs`

- **ACTION**: UPDATE startup to use validated production configuration.
- **IMPLEMENT**: Immediately after `var builder = WebApplication.CreateBuilder(args);`, call the new validator: `var productionConfiguration = ProductionConfigurationValidator.Validate(builder.Configuration, builder.Environment);`. Replace inline `Cors:AllowedOrigins` binding with `productionConfiguration.CorsAllowedOrigins`. Replace `builder.Configuration.GetConnectionString("DefaultConnection")` in `UseNpgsql(...)` with `productionConfiguration.DefaultConnection`. Preserve `FrontendCorsPolicy`, middleware order, Identity options, rate limiting, and the existing `Testing` HTTPS redirection guard.
- **MIRROR**: `server/FinancesControl.API/Program.cs:19-40` - top-level startup style and named CORS policy.
- **IMPORTS**: Existing `using FinancesControl.API.Common;` should already cover the new validator; add only if absent.
- **GOTCHA**: Validate before CORS and EF service registration so failures happen during startup composition, not first request or first database call.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 3: UPDATE `server/FinancesControl.API/appsettings.json`

- **ACTION**: UPDATE committed base configuration shape.
- **IMPLEMENT**: Replace `"AllowedHosts": "*"` with a non-wildcard placeholder such as `"AllowedHosts": "api.example.com"`. Add `"ConnectionStrings": { "DefaultConnection": "" }` and `"Cors": { "AllowedOrigins": [] }` so required sections are visible without committing secrets. Keep JSON valid and comment-free.
- **MIRROR**: `server/FinancesControl.API/appsettings.Development.json:1-16` - existing JSON shape for connection strings and CORS origin arrays.
- **GOTCHA**: Do not put a real production connection string in committed config. The empty base value is intentional because production should supply it through environment/deployment configuration and fail fast when absent.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 4: CREATE `server/FinancesControl.Tests/Common/ProductionConfigurationTests.cs`

- **ACTION**: CREATE focused unit tests for the configuration helper.
- **IMPLEMENT**: Add tests named `Validate_WithProductionConfig_ReturnsNormalizedValues`, `Validate_WithMissingDefaultConnectionInProduction_ThrowsInvalidOperationException`, `Validate_WithMissingCorsOriginsInProduction_ThrowsInvalidOperationException`, `Validate_WithWildcardAllowedHostsInProduction_ThrowsInvalidOperationException`, `Validate_WithWildcardCorsOriginInProduction_ThrowsInvalidOperationException`, and `Validate_WithTestingEnvironment_AllowsMissingProductionValues`. Build configuration using `new ConfigurationBuilder().AddInMemoryCollection(...)`; create a small test `IHostEnvironment` implementation or substitute with NSubstitute. Assert exception messages include the offending key (`ConnectionStrings:DefaultConnection`, `Cors:AllowedOrigins`, or `AllowedHosts`) and never include secret values.
- **MIRROR**: `server/FinancesControl.Tests/Common/IdentityOptionsConfigurationTests.cs:7-57` - direct static helper tests; `server/FinancesControl.Tests/Common/HttpsRedirectionTestingEnvironmentTests.cs:33-52` - environment name usage.
- **IMPORTS**: `using FinancesControl.API.Common;`, `using FluentAssertions;`, `using Microsoft.Extensions.Configuration;`, `using Microsoft.Extensions.FileProviders;`, `using Microsoft.Extensions.Hosting;`, plus NSubstitute only if used for `IHostEnvironment`.
- **GOTCHA**: Avoid `WebApplicationFactory` for most validation cases; direct helper tests are faster and do not require replacing the database provider. Add one integration test only if direct tests cannot prove `Program.cs` uses the helper.
- **VALIDATE**: `dotnet test server/FinancesControl.sln --filter FullyQualifiedName~ProductionConfigurationTests`

### `[x]` Task 5: UPDATE `docs/features/backend-production-readiness-fixes.md`

- **ACTION**: UPDATE deployment/configuration notes for this checklist item, or add a nearby `docs/features/backend-production-configuration.md` if editing the checklist would make it too noisy.
- **IMPLEMENT**: Document required production keys: `ASPNETCORE_ENVIRONMENT=Production`, `ConnectionStrings__DefaultConnection`, `Cors__AllowedOrigins__0` (and additional numbered origins as needed), and `AllowedHosts` with concrete host names and no scheme/port. State that secrets must come from environment variables, deployment secrets, or a secret store, not committed JSON. Mention that CORS origins need scheme + host + optional port and must not end with `/`.
- **MIRROR**: `docs/features/backend-production-readiness-fixes.md:43-51` - existing concise checklist language.
- **GOTCHA**: AGENTS.md says not to modify docs unless explicitly asked. This plan is allowed to include a docs task because the source requirement explicitly says to document required environment variables.
- **VALIDATE**: `dotnet test server/FinancesControl.sln --filter FullyQualifiedName~ProductionConfigurationTests`

---

## Testing Strategy

### Unit Tests to Write

| Test File | Test Cases | Validates |
| --------- | ---------- | --------- |
| `server/FinancesControl.Tests/Common/ProductionConfigurationTests.cs` | Valid production config returns connection string and origins | Helper exposes normalized values for `Program.cs` |
| `server/FinancesControl.Tests/Common/ProductionConfigurationTests.cs` | Missing `ConnectionStrings:DefaultConnection` throws | Production cannot start without DB config |
| `server/FinancesControl.Tests/Common/ProductionConfigurationTests.cs` | Missing/empty `Cors:AllowedOrigins` throws | Production cannot silently use empty CORS origins |
| `server/FinancesControl.Tests/Common/ProductionConfigurationTests.cs` | `AllowedHosts` equal to `*` throws | Production cannot use wildcard host filtering |
| `server/FinancesControl.Tests/Common/ProductionConfigurationTests.cs` | `Cors:AllowedOrigins` containing `*` or trailing slash throws | Credentialed CORS uses exact safe origins |
| `server/FinancesControl.Tests/Common/ProductionConfigurationTests.cs` | `Testing` environment allows missing values | Existing integration tests can continue overriding DB/CORS safely |

### Edge Cases Checklist

- [ ] `ConnectionStrings:DefaultConnection` is missing, empty, or whitespace.
- [ ] `Cors:AllowedOrigins` section is missing, empty, whitespace-only, contains `"*"`, or has trailing slash.
- [ ] `AllowedHosts` is missing, empty, whitespace-only, or exactly `"*"`.
- [ ] Multiple CORS origins are preserved in order after trimming.
- [ ] Development and Testing environments do not fail because local/test config is intentionally incomplete.
- [ ] Exception messages name keys but do not echo the connection string.

---

## Validation Commands

Validation loop: the plan is not complete until every command below passes (exit 0). On any failure, fix the cause and re-run - loop until all pass. If a check is genuinely impossible, mark it `[f]`, note why in Agent Notes, and move on.

### Level 1: STATIC_ANALYSIS

```bash
dotnet build server/FinancesControl.sln
```

**EXPECT**: Exit 0. Existing warnings may remain if unrelated; no new warnings from this change.

### Level 2: UNIT_TESTS

```bash
dotnet test server/FinancesControl.sln --filter FullyQualifiedName~ProductionConfigurationTests
```

**EXPECT**: All new production configuration tests pass.

### Level 3: FULL_SUITE

```bash
dotnet test server/FinancesControl.sln
```

**EXPECT**: All backend tests pass.

### Level 4: DATABASE_VALIDATION (if schema changes)

Not applicable. This plan changes configuration/startup only and does not alter database schema.

### Level 5: BROWSER_VALIDATION (if UI changes)

Not applicable. This plan has no frontend/UI changes.

### Level 6: MANUAL_VALIDATION

1. Run the API in `Development` and confirm local startup still works with `appsettings.Development.json`.
2. Run with `ASPNETCORE_ENVIRONMENT=Production` and no `ConnectionStrings__DefaultConnection`; confirm startup fails with a clear message naming `ConnectionStrings:DefaultConnection`.
3. Run with a dummy production connection string, `AllowedHosts=*`, and valid CORS; confirm startup fails with a clear message naming `AllowedHosts`.
4. Run with valid dummy production values for all required keys; confirm startup passes configuration validation. A real database may still be required for requests that touch persistence.

---

## Acceptance Criteria

- [x] Production-like environments fail fast when `ConnectionStrings:DefaultConnection` is missing or whitespace.
- [x] Production-like environments fail fast when `Cors:AllowedOrigins` is missing, empty, wildcard, or contains trailing-slash origins.
- [x] Production-like environments fail fast when `AllowedHosts` is missing, empty, or exactly `"*"`.
- [x] Development and Testing environments preserve current local/test behavior.
- [x] `Program.cs` uses one validated connection string value for EF Core and one validated origins array for CORS.
- [x] Committed config shows required keys without real secrets and without wildcard `AllowedHosts`.
- [x] Required production environment variables/deployment keys are documented.
- [x] Level 1-3 validation commands pass with exit 0.

---

## Completion Checklist

- [x] All tasks completed in dependency order
- [x] Each task validated immediately after completion
- [x] Level 1: Static analysis/build passes
- [x] Level 2: Focused unit tests pass
- [x] Level 3: Full backend test suite passes
- [x] Level 4: Database validation marked not applicable
- [x] Level 5: Browser validation marked not applicable
- [x] All acceptance criteria met

---

## Risks and Mitigations

| Risk | Likelihood | Impact | Mitigation |
| ---- | ---------- | ------ | ---------- |
| Startup validation breaks existing integration tests | MED | MED | Explicitly bypass strict production validation for `Testing`, mirroring current `UseEnvironment("Testing")` test setup |
| Production deploys fail after implementation because required env vars are absent | MED | HIGH | Document exact keys and fail with clear messages before serving traffic |
| Connection string accidentally appears in logs or exception messages | LOW | HIGH | Exception messages must name the key only, never the configured value |
| CORS origins are normalized incorrectly | MED | MED | Trim whitespace, reject trailing slash, and test multiple origins |
| `AllowedHosts` placeholder is mistaken for real production config | LOW | MED | Documentation must state production must override committed placeholder values |

---

## Questionables

<details>
<summary>Should strict validation apply to Staging as well as Production?</summary>

Assumption: yes. The helper should be strict for every environment except `Development` and `Testing`, because staging should be production-like and catch deployment config issues before release.

</details>

<details>
<summary>Should the implementation use Options validation or a direct helper?</summary>

Assumption: use a direct helper first. Microsoft documents `ValidateOnStart`, but this repo's closest pattern is small static helpers in `Common` tested directly. A helper also makes it easy for `Program.cs` to reuse normalized CORS origins and the connection string without adding extra option plumbing.

</details>

---

## Agent Notes

APPROACH_CHOSEN: A small `Common` startup helper that validates production configuration and returns normalized values consumed by `Program.cs`.

RATIONALE: Existing backend hardening work introduced `IdentityOptionsConfiguration` as a static `Common` helper and tested it directly. This mirrors that style, keeps startup code readable, avoids adding new packages, and gives direct tests for failure modes. `Program.cs` is already the central startup integration point for CORS, EF Core, Identity, rate limiting, and environment-specific middleware.

ALTERNATIVES_REJECTED:

- Rely only on deployment documentation: rejected because the source task explicitly asks to fail fast when required config is missing.
- Use `AllowAnyOrigin` for production convenience: rejected because the app calls `AllowCredentials()` and Microsoft documents wildcard origins with credentials as invalid/risky.
- Add cloud-provider-specific secret manager integration: rejected as out of scope and not documented yet for this repo.
- Convert all config to strongly typed options now: rejected as broader than needed; direct helper gives the required fail-fast behavior with less startup churn.

Codebase discovery highlights:

| Category | File:Lines | Pattern Description | Code Snippet |
| -------- | ---------- | ------------------- | ------------ |
| CONFIG | `server/FinancesControl.API/Program.cs:30-38` | Current CORS origins are read inline and used with credentials | `.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];` |
| CONFIG | `server/FinancesControl.API/Program.cs:55-57` | EF Core reads `DefaultConnection` during service registration | `options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))` |
| CONFIG | `server/FinancesControl.API/Data/AppDbConnectionFactory.cs:16-17` | Dapper uses the same connection string key | `new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"))` |
| CONFIG | `server/FinancesControl.API/appsettings.json:8` | Base config currently permits every host | `"AllowedHosts": "*"` |
| TESTS | `server/FinancesControl.Tests/Common/HttpsRedirectionTestingEnvironmentTests.cs:37-50` | Integration tests set `Testing` and replace the DbContext | `builder.UseEnvironment("Testing"); ... options.UseInMemoryDatabase(databaseName)` |
| DOCS | `docs/features/backend-production-readiness-fixes.md:45-51` | Source checklist item describes this exact risk and hint | `DefaultConnection` and `Cors:AllowedOrigins` missing; `AllowedHosts` wildcard |

External research findings:

- ASP.NET Core configuration keys are hierarchical; environment variables commonly use `__` for nested keys, so document `ConnectionStrings__DefaultConnection` and `Cors__AllowedOrigins__0`.
- Options validation with `ValidateOnStart` is available, but local patterns favor static helpers and direct unit tests.
- CORS docs state allowed origins should be explicit, origin URLs must not end in `/`, and wildcard origins are unsafe/invalid when credentials are enabled.
- Host filtering docs state `AllowedHosts` values should exclude ports and `*` allows all non-empty hosts.

Confidence score: 8/10. The plan uses close local patterns and focused tests. The main uncertainty is whether the implementation should introduce formal options binding; the direct-helper approach is intentionally smaller and easier to verify in this repo.

---

## Amendments

<details>
<summary>2026-07-19T00:00:00-03:00 - Initial plan created</summary>

Created the implementation plan from `PRPs/tasks/add-production-configuration-requirements.md` after codebase exploration, local docs review, and official Microsoft documentation research.

</details>

<details>
<summary>2026-07-19T05:38:15-03:00 - Implementation completed</summary>

Implemented `ProductionConfigurationValidator`, wired `Program.cs` to reuse validated connection string and CORS origins, updated base appsettings to show required non-secret keys, added focused production configuration tests, and documented deployment environment variables. Deviation: `AllowedHosts` uses `api.example.com;localhost` instead of only `api.example.com` so existing `WebApplicationFactory` tests using localhost are not rejected by ASP.NET host filtering.

</details>
