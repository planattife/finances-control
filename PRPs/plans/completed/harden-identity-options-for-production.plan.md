# Feature: Harden Identity Options For Production

## Summary

Make ASP.NET Core Identity security posture explicit for the finance API by configuring `IdentityOptions` instead of relying on framework defaults. The implementation should add a small testable configuration helper, call it from `AddIdentityApiEndpoints<IdentityUser>(...)` in `Program.cs`, and cover the configured password, lockout, sign-in, and user options with focused backend tests.

## User Story

As a finance app user
I want account authentication rules to be intentionally hardened
So that my financial workspace has stronger protection against weak passwords, duplicate email identities, and repeated sign-in attacks

## Problem Statement

`Program.cs` currently calls `AddIdentityApiEndpoints<IdentityUser>()` without explicit `IdentityOptions`, so password, lockout, sign-in, and user behavior depend on ASP.NET Core defaults. This is testable by resolving or invoking Identity option configuration and verifying the app explicitly sets all required production values.

## Solution Statement

Create `IdentityOptionsConfiguration` in the API project with one `Configure(IdentityOptions options)` method. Update `Program.cs` to pass that method to `AddIdentityApiEndpoints<IdentityUser>(IdentityOptionsConfiguration.Configure)`, preserving the existing EF Identity store and mapped Identity endpoints. Add unit tests that construct `IdentityOptions`, call the helper, and assert each hardened setting.

## Metadata

| Field            | Value |
| ---------------- | ----- |
| Type             | ENHANCEMENT |
| Complexity       | LOW |
| Systems Affected | Backend API startup, ASP.NET Core Identity configuration, backend tests |
| Dependencies     | Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.23, Microsoft.EntityFrameworkCore 8.0.23, xUnit 2.9.2, FluentAssertions 4.19.4 |
| Estimated Tasks  | 3 |

---

## Lifecycle (append-only)

- **Created:** 2026-07-16T00:00:00-03:00
- **Modified:** 2026-07-16T00:00:00-03:00 - Initial plan created from `PRPs/tasks/harden-identity-options-for-production.md`
- **Modified:** 2026-07-16T16:01:54-03:00 - Implemented Identity options helper, wired startup registration, and added focused tests
- **Commits:** None yet
- **Commits:** Not committed in this session
- **Agent / Session:** Codex GPT-5 planning session, 2026-07-16
- **Agent / Session:** Codex GPT-5 implementation session, 2026-07-16
- **Back refs:** `PRPs/tasks/harden-identity-options-for-production.md` - source task; `docs/features/backend-production-readiness-fixes.md` - production-readiness checklist item
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
|   Identity API request ---> AddIdentityApiEndpoints<IdentityUser>() ---> Uses |
|   framework default password, lockout, sign-in, and user options              |
|                                                                              |
|   USER_FLOW: User registers or signs in through mapped Identity API endpoints |
|   PAIN_POINT: Security posture is implicit and can drift with framework       |
|   defaults; finance-specific intent is not documented in code or tests.       |
|   DATA_FLOW: Program.cs registers Identity services -> EF store persists      |
|   IdentityUser state -> MapIdentityApi exposes auth endpoints.                |
|                                                                              |
+------------------------------------------------------------------------------+
```

### After State

```
+------------------------------------------------------------------------------+
|                                  AFTER STATE                                  |
+------------------------------------------------------------------------------+
|                                                                              |
|   Identity API request ---> AddIdentityApiEndpoints<IdentityUser>(Configure)  |
|             |                                                                |
|             v                                                                |
|   IdentityOptionsConfiguration.Configure sets explicit production rules       |
|                                                                              |
|   USER_FLOW: User registers or signs in through the same endpoints, now under |
|   explicit password, lockout, sign-in, and unique-email rules.                |
|   VALUE_ADD: Account protection is deliberate, reviewable, and regression     |
|   tested for production readiness.                                            |
|   DATA_FLOW: Program.cs registers configured Identity services -> EF store    |
|   persists IdentityUser state -> MapIdentityApi exposes unchanged endpoints.  |
|                                                                              |
+------------------------------------------------------------------------------+
```

### Interaction Changes

| Location | Before | After | User Impact |
|----------|--------|-------|-------------|
| `Program.cs` Identity registration | Uses default Identity options | Uses explicit production options via `IdentityOptionsConfiguration.Configure` | Users face stronger password and lockout rules without endpoint shape changes |
| Identity registration endpoint | Accepts passwords according to defaults | Requires at least 12 characters, digit, lowercase, uppercase, non-alphanumeric, and 4 unique characters | Weak passwords are rejected before account creation |
| Identity sign-in endpoint | Lockout/sign-in policy depends on defaults | New users can be locked out after 5 failed attempts for 15 minutes; unique email is required | Repeated guessing is throttled and duplicate email accounts are prevented |

---

## Mandatory Reading

**CRITICAL: Implementation agent MUST read these files before starting any task:**

| Priority | File | Lines | Why Read This |
|----------|------|-------|---------------|
| P0 | `server/FinancesControl.API/Program.cs` | 23-64, 87-109 | Existing startup composition, service-registration lambda style, and current Identity registration to update |
| P0 | `PRPs/tasks/harden-identity-options-for-production.md` | all | Source requirement and expected direction |
| P1 | `docs/backend-testing-guidelines.md` | all | Backend test naming, structure, and validation command |
| P1 | `server/FinancesControl.Tests/Common/GlobalExceptionHandlerMiddlewareTests.cs` | 9-79 | xUnit plus FluentAssertions style for shared infrastructure tests |
| P1 | `server/FinancesControl.API/Data/FinancesDbContext.cs` | 1-12 | Confirms Identity uses the existing EF `IdentityDbContext` store |
| P2 | `server/FinancesControl.API/Modules/Auth/Controllers/AuthController.cs` | 1-25 | Confirms current auth surface depends on `SignInManager<IdentityUser>` and should remain unchanged |

**External Documentation:**
| Source | Section | Why Needed |
|--------|---------|------------|
| [Configure ASP.NET Core Identity, ASP.NET Core 8.0](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-8.0#identity-options) | Identity options | Confirms `IdentityOptions` is the supported place for password, lockout, sign-in, and user settings |
| [Configure ASP.NET Core Identity, ASP.NET Core 8.0](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-8.0#password) | Password | Confirms password properties and defaults, including `RequiredLength` and `RequiredUniqueChars` |
| [Configure ASP.NET Core Identity, ASP.NET Core 8.0](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-8.0#lockout) | Lockout | Confirms lockout properties and that successful authentication resets failed attempts |
| [AddIdentityApiEndpoints API docs](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.identityservicecollectionextensions.addidentityapiendpoints?view=aspnetcore-8.0) | Overloads | Confirms there is an overload accepting `Action<IdentityOptions>` for API endpoint Identity |

---

## Patterns to Mirror

**NAMING_CONVENTION:**

```csharp
// SOURCE: server/FinancesControl.API/Modules/Accounts/Services/AccountService.cs:8
// COPY THIS PATTERN:
public class AccountService : IAccountService
```

**ERROR_HANDLING:**

```csharp
// SOURCE: server/FinancesControl.API/Common/Exceptions/DomainValidationException.cs:3-6
// COPY THIS PATTERN:
public class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message)
    {
    }
}
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

**SERVICE_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Program.cs:25-39
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

**TEST_STRUCTURE:**

```csharp
// SOURCE: server/FinancesControl.Tests/Common/GlobalExceptionHandlerMiddlewareTests.cs:11-23
// COPY THIS PATTERN:
[Theory]
[InlineData(typeof(DomainValidationException), 400, "Invalid amount.")]
[InlineData(typeof(BusinessRuleException), 400, "Card is inactive.")]
[InlineData(typeof(ResourceNotFoundException), 404, "Account not found.")]
[InlineData(typeof(AuthenticationContextException), 401, "User context is not available.")]
public async Task InvokeAsync_WithApplicationException_ReturnsMappedErrorResponse(Type exceptionType, int statusCode, string message)
{
    var exception = (Exception)Activator.CreateInstance(exceptionType, message)!;

    var context = await InvokeMiddleware(exception);

    context.Response.StatusCode.Should().Be(statusCode);
    context.Response.ContentType.Should().Be("application/json");
}
```

---

## Files to Change

| File | Action | Justification |
| ---- | ------ | ------------- |
| `server/FinancesControl.API/Common/IdentityOptionsConfiguration.cs` | CREATE | Centralizes explicit production Identity options in a testable helper |
| `server/FinancesControl.API/Program.cs` | UPDATE | Passes the explicit options configurator to `AddIdentityApiEndpoints<IdentityUser>()` |
| `server/FinancesControl.Tests/Common/IdentityOptionsConfigurationTests.cs` | CREATE | Verifies password, lockout, sign-in, and user options are intentional and stable |

---

## NOT Building (Scope Limits)

Explicit exclusions to prevent scope creep:

- Email confirmation delivery, token email templates, SMTP/provider integration, and account-confirmation UI are out of scope because this task only asks for Identity option hardening.
- Custom `IdentityUser` subclasses, new auth endpoints, and schema migrations are out of scope because the existing app already uses framework `IdentityUser` and Identity tables include the required lockout/email fields.
- Cookie/token lifetime policy is out of scope unless a separate production-readiness task calls for it.

---

## Step-by-Step Tasks

Execute in order. Each task is atomic and independently verifiable.

**Status markers** - prefix EVERY task header with one; the build agent updates it inline as it works: `[ ]` idle, `[wip]` in progress, `[x]` complete, `[f]` failed. All tasks start `[ ]`. If a task cannot be made to pass, mark it `[f]`, record why in Agent Notes, and move on if the rest of the plan can still proceed.

### `[x]` Task 1: CREATE `server/FinancesControl.API/Common/IdentityOptionsConfiguration.cs`

- **ACTION**: CREATE a static configuration helper.
- **IMPLEMENT**: Add `namespace FinancesControl.API.Common;` and `public static class IdentityOptionsConfiguration` with `public static void Configure(IdentityOptions options)`. Set `options.Password.RequiredLength = 12`, `RequiredUniqueChars = 4`, `RequireDigit = true`, `RequireLowercase = true`, `RequireUppercase = true`, `RequireNonAlphanumeric = true`; `options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15)`, `MaxFailedAccessAttempts = 5`, `AllowedForNewUsers = true`; `options.User.RequireUniqueEmail = true`; `options.SignIn.RequireConfirmedEmail = false`, `RequireConfirmedAccount = false`, and `RequireConfirmedPhoneNumber = false`.
- **MIRROR**: `server/FinancesControl.API/Common/Middleware/GlobalExceptionHandlerMiddleware.cs:6-13` - use the existing `FinancesControl.API.Common` namespace and simple public class style.
- **IMPORTS**: `using Microsoft.AspNetCore.Identity;`
- **GOTCHA**: Microsoft docs show `IdentityOptions` must be configured after adding Identity services, and `AddIdentityApiEndpoints<TUser>` has an overload accepting `Action<IdentityOptions>`; use the helper as that callback instead of a separate late `Configure<IdentityOptions>` registration.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 2: UPDATE `server/FinancesControl.API/Program.cs`

- **ACTION**: UPDATE the Identity registration.
- **IMPLEMENT**: Replace `builder.Services.AddIdentityApiEndpoints<IdentityUser>()` with `builder.Services.AddIdentityApiEndpoints<IdentityUser>(IdentityOptionsConfiguration.Configure)` and keep `.AddEntityFrameworkStores<FinancesDbContext>();` unchanged. Do not move `MapIdentityApi<IdentityUser>()`, `UseAuthentication()`, or `UseAuthorization()`.
- **MIRROR**: `server/FinancesControl.API/Program.cs:42-52` - follow the existing pattern of passing named option configuration into service registration.
- **IMPORTS**: No new import should be needed because `Program.cs` already has `using FinancesControl.API.Common;` and `using Microsoft.AspNetCore.Identity;`.
- **GOTCHA**: Preserve the existing `IdentityUser` contract; changing to a custom user type would ripple into migrations, `AuthController`, and domain user-id claims.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 3: CREATE `server/FinancesControl.Tests/Common/IdentityOptionsConfigurationTests.cs`

- **ACTION**: CREATE focused unit tests for the option helper.
- **IMPLEMENT**: Add a test class in `namespace FinancesControl.Tests.Common;`. Use `[Fact]` tests named `Configure_WithDefaultOptions_SetsProductionPasswordOptions`, `Configure_WithDefaultOptions_SetsProductionLockoutOptions`, `Configure_WithDefaultOptions_RequiresUniqueEmail`, and `Configure_WithDefaultOptions_LeavesConfirmedAccountDisabledUntilEmailConfirmationExists`. Each test should instantiate `var options = new IdentityOptions();`, call `IdentityOptionsConfiguration.Configure(options);`, and assert the exact values with FluentAssertions.
- **MIRROR**: `server/FinancesControl.Tests/Common/GlobalExceptionHandlerMiddlewareTests.cs:9-23` - class placement, xUnit attributes, and FluentAssertions style.
- **IMPORTS**: `using FinancesControl.API.Common;`, `using FluentAssertions;`, `using Microsoft.AspNetCore.Identity;`
- **GOTCHA**: Do not test through `WebApplicationFactory` unless the project already has that package; the test project does not currently reference `Microsoft.AspNetCore.Mvc.Testing`.
- **VALIDATE**: `dotnet test server/FinancesControl.sln --filter IdentityOptionsConfigurationTests`

---

## Testing Strategy

### Unit Tests to Write

| Test File | Test Cases | Validates |
| --------- | ---------- | --------- |
| `server/FinancesControl.Tests/Common/IdentityOptionsConfigurationTests.cs` | `Configure_WithDefaultOptions_SetsProductionPasswordOptions` | Password complexity and minimum length are explicit |
| `server/FinancesControl.Tests/Common/IdentityOptionsConfigurationTests.cs` | `Configure_WithDefaultOptions_SetsProductionLockoutOptions` | Lockout duration, failed-attempt threshold, and new-user lockout behavior are explicit |
| `server/FinancesControl.Tests/Common/IdentityOptionsConfigurationTests.cs` | `Configure_WithDefaultOptions_RequiresUniqueEmail` | Duplicate email identities are disallowed |
| `server/FinancesControl.Tests/Common/IdentityOptionsConfigurationTests.cs` | `Configure_WithDefaultOptions_LeavesConfirmedAccountDisabledUntilEmailConfirmationExists` | Confirmed-account policy is explicit and matches current lack of email confirmation flow |

### Edge Cases Checklist

- [ ] Passwords shorter than 12 characters are rejected by Identity's configured validators.
- [ ] Passwords missing digit, lowercase, uppercase, or non-alphanumeric characters are rejected by Identity's configured validators.
- [ ] Passwords with fewer than 4 unique characters are rejected.
- [ ] Users are lockout-eligible immediately after registration.
- [ ] Five failed sign-in attempts trigger the configured lockout threshold.
- [ ] Duplicate emails are rejected by Identity user validation.
- [ ] Confirmed-account enforcement remains disabled until email confirmation is implemented.

---

## Validation Commands

**Validation loop:** the plan is not complete until every command below passes (exit 0). On any failure, fix the cause and re-run - loop until all pass. If a check is genuinely impossible, mark it `[f]`, note why in Agent Notes, and move on.

### Level 1: STATIC_ANALYSIS

```bash
dotnet build server/FinancesControl.sln
```

**EXPECT**: Exit 0, no compiler errors or warnings introduced by the Identity configuration change

### Level 2: UNIT_TESTS

```bash
dotnet test server/FinancesControl.sln --filter IdentityOptionsConfigurationTests
```

**EXPECT**: All new Identity option tests pass

### Level 3: FULL_SUITE

```bash
dotnet test server/FinancesControl.sln
```

**EXPECT**: All backend tests pass with no regression

### Level 4: DATABASE_VALIDATION (if schema changes)

Not applicable. This plan changes runtime Identity options only and does not add migrations or tables.

### Level 5: BROWSER_VALIDATION (if UI changes)

Not applicable. This plan does not change frontend UI.

### Level 6: MANUAL_VALIDATION

1. Run the API locally with the normal backend launch profile.
2. Attempt to register with an 11-character password and confirm Identity rejects it.
3. Attempt to register with a 12+ character password missing a required character class and confirm Identity rejects it.
4. Attempt to register with a 12+ character password satisfying all configured rules and confirm the endpoint can create the user.
5. Attempt to register a second user with the same email and confirm Identity rejects the duplicate email.

---

## Acceptance Criteria

- [ ] `Program.cs` passes explicit Identity option configuration into `AddIdentityApiEndpoints<IdentityUser>()`.
- [ ] Password options explicitly require 12+ characters, digit, lowercase, uppercase, non-alphanumeric, and at least 4 unique characters.
- [ ] Lockout options explicitly allow lockout for new users, lock out after 5 failed attempts, and use a 15-minute lockout window.
- [ ] User options explicitly require unique email.
- [ ] Sign-in confirmation requirements are explicit, with confirmed email/account disabled until an email confirmation flow exists.
- [ ] New tests cover all configured option groups.
- [ ] Level 1-3 validation commands pass with exit 0.

---

## Completion Checklist

- [ ] All tasks completed in dependency order
- [ ] Each task validated immediately after completion
- [ ] Level 1: Static analysis build passes
- [ ] Level 2: Focused unit tests pass
- [ ] Level 3: Full backend test suite passes
- [ ] Level 4: Database validation marked not applicable
- [ ] Level 5: Browser validation marked not applicable
- [ ] All acceptance criteria met

---

## Risks and Mitigations

| Risk | Likelihood | Impact | Mitigation |
| ---- | ---------- | ------ | ---------- |
| Existing development/test users have passwords that no longer satisfy production complexity rules | MED | MED | Document this as a forward-only registration/password-reset behavior; existing password hashes are not revalidated until a password change/reset |
| Enabling confirmed-account sign-in without email confirmation would block new users | HIGH | HIGH | Keep `RequireConfirmedEmail` and `RequireConfirmedAccount` false in this task, and make the decision explicit in code/tests |
| Testing through the whole web host would require new test infrastructure | MED | LOW | Keep the configuration in a small static helper and unit test it directly |
| Option values may be debated later | MED | LOW | Centralize all values in one helper and cover them with direct tests so policy changes are intentional |

---

## Questionables

_Include this section whenever a decision was assumed rather than certain (and whenever the confidence score is below 8). Surface open decisions here instead of silently deciding - one collapsible entry per open question, with the assumption you took so a human can confirm or correct it._

<details>
<summary>Should confirmed account or confirmed email be required now?</summary>

Assumption: keep `RequireConfirmedAccount`, `RequireConfirmedEmail`, and `RequireConfirmedPhoneNumber` set to `false` because the repository does not currently show an email confirmation delivery flow. This still satisfies the task by making the policy explicit. When email confirmation is implemented, this helper and its tests should be updated to require confirmed accounts.

</details>

<details>
<summary>Are the exact password and lockout values product-approved?</summary>

Assumption: use a production-leaning policy of 12 minimum characters, all core complexity classes, 4 unique characters, 5 failed attempts, and a 15-minute lockout. The source task does not prescribe exact values, so the plan chooses conservative values appropriate for a personal finance application while keeping them centralized and easy to revise.

</details>

---

## Agent Notes

Codebase discovery found no existing explicit `IdentityOptions` configuration. The closest local pattern is startup service configuration via lambdas in `Program.cs`, especially CORS and Swagger. A named helper is intentionally introduced for testability; putting the lambda inline would satisfy runtime behavior but would make regression tests either brittle or dependent on adding `WebApplicationFactory` infrastructure.

External research:

- Microsoft Learn for ASP.NET Core 8.0 states Identity options cover password policy, lockout, sign-in, and user settings, and lists the defaults. This plan hardens beyond the default 6-character minimum while preserving the documented option surface.
- Microsoft Learn API docs list `AddIdentityApiEndpoints<TUser>(IServiceCollection, Action<IdentityOptions>)`, which fits the current minimal API Identity endpoint pattern better than switching to `AddDefaultIdentity` or hand-written auth endpoints.

Approach chosen: named static configurator plus direct `AddIdentityApiEndpoints` callback.

Alternatives rejected:

- Inline lambda in `Program.cs`: rejected because testing exact values would be awkward without adding web-host test infrastructure.
- `builder.Services.Configure<IdentityOptions>(...)` after Identity registration: valid per docs, but the overload on the current API endpoint registration keeps the configuration attached to the service being configured.
- Enable confirmed email immediately: rejected because no email confirmation flow is visible in the repo, and it would likely block registration/sign-in.

Confidence score: 9/10. The implementation is small, anchored to current startup patterns, uses official ASP.NET Core 8 option APIs, and has direct tests. The only meaningful uncertainty is final product approval of exact option values.

---

## Amendments

_Append-only history of changes made **after** this plan was first built (newest at the bottom). The build and update steps add entries here; never edit or remove existing ones._

<details>
<summary>2026-07-16T00:00:00-03:00 - Initial plan</summary>

Created the first implementation plan for hardening ASP.NET Core Identity options for production.

</details>

<details>
<summary>2026-07-16T16:01:54-03:00 - Implemented Identity options hardening</summary>

Added a testable `IdentityOptionsConfiguration` helper, passed it into `AddIdentityApiEndpoints<IdentityUser>`, and covered password, lockout, unique-email, and sign-in confirmation options with focused unit tests. Implementation matched the plan; manual API registration checks were not run.

</details>
