# Feature: Limit Pagination Page Size

## Summary

Add a documented maximum page size to the shared backend `PaginationParams` type and clamp positive `PageSize` values above that maximum before they reach list services or Dapper SQL `LIMIT` parameters. This keeps the existing invalid-value behavior for negative and zero values, protects accounts, banks, cards, categories, and transactions through one central change, and adds focused common unit tests for the pagination normalization contract.

## User Story

As an authenticated user
I want list endpoints to enforce a bounded page size
So that normal pagination remains usable while oversized requests cannot create avoidable backend load

## Problem Statement

Any authenticated user can currently pass an arbitrarily large `PageSize` query value to list endpoints. Because `PaginationParams.PageSize` is passed directly into SQL `LIMIT @PageSize`, a request such as `?PageSize=1000000` can ask the database, API process, and response serialization path to handle an excessive number of rows.

## Solution Statement

Update `server/FinancesControl.API/Common/PaginationParams.cs` so `PageSize` keeps defaulting values below `1` to `10`, preserves normal positive values up to the maximum, and clamps values above a documented `MaxPageSize` of `100`. Add `server/FinancesControl.Tests/Common/PaginationParamsTests.cs` with direct unit coverage for default, negative, zero, valid positive, max, oversized, and offset behavior.

## Metadata

| Field            | Value                                             |
| ---------------- | ------------------------------------------------- |
| Type             | BUG_FIX |
| Complexity       | LOW |
| Systems Affected | Backend API common pagination, list endpoint query binding, backend tests |
| Dependencies     | .NET 8, xUnit 2.9.2, FluentAssertions 4.19.4 |
| Estimated Tasks  | 3 |

---

## Lifecycle (append-only)

- **Created:** 2026-07-18T16:57:20.0578704-03:00
- **Modified:** 2026-07-18T16:57:20.0578704-03:00 - Initial plan created from `PRPs/tasks/limit-pagination-page-size.md`.
- **Modified:** 2026-07-18T17:09:22.4970875-03:00 - Implemented pagination page size clamp, tests, validation, and report.
- **Commits:** None yet
- **Agent / Session:** Codex GPT-5 planning session, 2026-07-18
- **Agent / Session:** Codex GPT-5 implementation session, 2026-07-18
- **Back refs:** `PRPs/tasks/limit-pagination-page-size.md` - task brief
- **Forward refs:** None yet

> **Append-only:** `Created` is set once; every other field is a list you only ever add to -- never overwrite or remove existing entries. Keep references bidirectional: when you add a back/forward ref here, add the reciprocal ref on the other plan.

---

## UX Design

### Before State

```text
+------------------------------------------------------------------------------+
|                                BEFORE STATE                                  |
+------------------------------------------------------------------------------+
|                                                                              |
|   Client/API user  ---->  GET /accounts?PageSize=1000000  ---->  Controller  |
|                                                                              |
|   Controller binds [FromQuery] PaginationParams                               |
|          |                                                                   |
|          v                                                                   |
|   PageSize setter accepts any positive value                                  |
|          |                                                                   |
|          v                                                                   |
|   Service calculates metadata with huge PageSize                              |
|          |                                                                   |
|          v                                                                   |
|   SQL query receives LIMIT @PageSize = 1000000                                |
|                                                                              |
|   USER_FLOW: User or client requests a large page from any list endpoint.     |
|   PAIN_POINT: Positive page sizes are unbounded and can increase load.        |
|   DATA_FLOW: query string -> PaginationParams -> service -> repository SQL.   |
|                                                                              |
+------------------------------------------------------------------------------+
```

### After State

```text
+------------------------------------------------------------------------------+
|                                 AFTER STATE                                  |
+------------------------------------------------------------------------------+
|                                                                              |
|   Client/API user  ---->  GET /accounts?PageSize=1000000  ---->  Controller  |
|                                                                              |
|   Controller binds [FromQuery] PaginationParams                               |
|          |                                                                   |
|          v                                                                   |
|   PageSize setter clamps positive values above MaxPageSize to 100             |
|          |                                                                   |
|          v                                                                   |
|   Service calculates metadata with bounded PageSize                           |
|          |                                                                   |
|          v                                                                   |
|   SQL query receives LIMIT @PageSize = 100                                    |
|                                                                              |
|   USER_FLOW: User or client can still request normal pages.                   |
|   VALUE_ADD: Oversized list requests are bounded consistently.                |
|   DATA_FLOW: query string -> clamped PaginationParams -> service -> SQL.      |
|                                                                              |
+------------------------------------------------------------------------------+
```

### Interaction Changes

| Location | Before | After | User Impact |
|----------|--------|-------|-------------|
| `GET /accounts`, `/banks`, `/cards`, `/transaction-categories`, `/transactions` with `[FromQuery] PaginationParams` | Any positive `PageSize` is accepted as-is. | `PageSize > 100` is normalized to `100`; `PageSize < 1` still defaults to `10`. | Normal pagination remains unchanged; abusive or accidental oversized requests are bounded. |
| `PaginationParams.GetOffset()` | Offset uses unbounded positive `PageSize`. | Offset uses the normalized, clamped `PageSize`. | Offset remains consistent with the actual SQL limit. |

---

## Mandatory Reading

**CRITICAL: Implementation agent MUST read these files before starting any task:**

| Priority | File | Lines | Why Read This |
|----------|------|-------|---------------|
| P0 | `server/FinancesControl.API/Common/PaginationParams.cs` | 3-25 | Central behavior to update; all list endpoints consume this type. |
| P0 | `PRPs/tasks/limit-pagination-page-size.md` | all | Original security/performance task scope and expected direction. |
| P1 | `server/FinancesControl.API/Modules/Accounts/Repository/SqlCommands/GetAllAccountsQuery.cs` | 8-25 | Representative Dapper query showing `PageSize` flowing into `LIMIT`. |
| P1 | `server/FinancesControl.API/Modules/Accounts/Services/AccountService.cs` | 67-86 | Representative service pagination metadata calculation. |
| P1 | `docs/backend-testing-guidelines.md` | all | Backend test naming, folder, framework, and validation conventions. |
| P2 | `server/FinancesControl.Tests/Accounts/Services/AccountServiceTests.cs` | 326-340 | Existing assertion style for invalid pagination defaulting. |
| P2 | `server/FinancesControl.Tests/Common/GlobalExceptionHandlerMiddlewareTests.cs` | 9-30 | Existing Common test namespace/class and `[Theory]` pattern. |
| P2 | `server/FinancesControl.Tests/Transactions/Repository/SqlCommands/TransactionSqlCommandTests.cs` | 11-23 | Existing pagination SQL parameter assertions. |

**External Documentation:**
| Source | Section | Why Needed |
|--------|---------|------------|
| [ASP.NET Core model binding, .NET 8/10 docs](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding) | Complex types | Confirms query/model binding targets public writable properties on complex types, so the `PageSize` setter is the central enforcement point. |
| [ASP.NET Core action return types, .NET 8 docs](https://learn.microsoft.com/en-us/aspnet/core/web-api/action-return-types?view=aspnetcore-8.0) | `ActionResult<T>` type | Confirms existing controller return pattern is standard and does not need to change for this normalization-only fix. |

---

## Patterns to Mirror

**NAMING_CONVENTION:**

```csharp
// SOURCE: server/FinancesControl.API/Common/PaginationParams.cs:3-25
// COPY THIS PATTERN:
public class PaginationParams
{
    private const int DefaultPageSize = 10;
    private const int DefaultPageNumber = 1;

    private int _pageSize = DefaultPageSize;
    private int _pageNumber = DefaultPageNumber;
}
```

**ERROR_HANDLING:**

```csharp
// SOURCE: server/FinancesControl.API/Common/PaginationParams.cs:11-20
// COPY THIS PATTERN:
public int PageSize
{
    get => _pageSize;
    set => _pageSize = value < 1 ? DefaultPageSize : value;
}
```

This class normalizes invalid pagination input instead of throwing or returning HTTP 400. Preserve that behavior and add clamping for positive oversized input.

**LOGGING_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Common/Middleware/GlobalExceptionHandlerMiddleware.cs:42-43
// COPY THIS PATTERN:
_logger.LogError(exception, "Unhandled exception while processing the request.");
_logger.LogInformation(exception, "Handled application exception while processing the request.");
```

No new logging is needed for this task because oversized page size is normal query input that should be silently normalized, matching current invalid pagination behavior.

**REPOSITORY_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Modules/Accounts/Repository/SqlCommands/GetAllAccountsQuery.cs:20-21
// COPY THIS PATTERN:
parameters.Add("@PageSize", paginationParams.PageSize);
parameters.Add("@Offset", paginationParams.GetOffset());
```

Do not update repository SQL command classes. They should continue to consume the already-normalized `PaginationParams`.

**SERVICE_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Modules/Accounts/Services/AccountService.cs:79-83
// COPY THIS PATTERN:
var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);
var hasNextPage = paginationParams.PageNumber < totalPages;
var hasPrevPage = paginationParams.PageNumber > 1;

response.SetPagination(totalCount, totalPages, paginationParams.PageNumber, hasNextPage, hasPrevPage);
```

Do not duplicate clamping in services. Services should keep using `paginationParams.PageSize`.

**TEST_STRUCTURE:**

```csharp
// SOURCE: server/FinancesControl.Tests/Accounts/Services/AccountServiceTests.cs:326-340
// COPY THIS PATTERN:
[Fact]
public async Task GetAll_WithInvalidPaginationParams_UsesDefaults()
{
    // Arrange
    var userId = "user-123";
    var paginationParams = new PaginationParams { PageSize = 0, PageNumber = -1 };

    // Assert
    paginationParams.PageSize.Should().Be(10);
    paginationParams.PageNumber.Should().Be(1);
}
```

Use `Fact` for one-off default behavior and `Theory` with `InlineData` for equivalent input-to-output cases.

---

## Files to Change

| File                             | Action | Justification                            |
| -------------------------------- | ------ | ---------------------------------------- |
| `server/FinancesControl.API/Common/PaginationParams.cs` | UPDATE | Add documented maximum page size and central clamp. |
| `server/FinancesControl.Tests/Common/PaginationParamsTests.cs` | CREATE | Add focused regression coverage for default, invalid, valid, max, oversized, and offset behavior. |

---

## NOT Building (Scope Limits)

Explicit exclusions to prevent scope creep:

- No controller changes: existing `[FromQuery] PaginationParams` binding already reaches the shared setter.
- No repository or SQL changes: repositories should keep using `paginationParams.PageSize` and `paginationParams.GetOffset()`.
- No API error response for oversized values: existing behavior normalizes invalid pagination input instead of rejecting requests.
- No configurable appsettings maximum: the task asks for a documented maximum such as `MaxPageSize = 100`; config-driven limits are not documented yet.
- No frontend changes: the server-side contract safely handles oversized input regardless of client behavior.

---

## Step-by-Step Tasks

Execute in order. Each task is atomic and independently verifiable.

**Status markers** -- prefix EVERY task header with one; the build agent updates it inline as it works: `[ ]` idle * `[wip]` in progress * `[x]` complete * `[f]` failed. All tasks start `[ ]`. If a task cannot be made to pass, mark it `[f]`, record why in Agent Notes, and move on if the rest of the plan can still proceed.

### `[x]` Task 1: UPDATE `server/FinancesControl.API/Common/PaginationParams.cs`

- **ACTION**: UPDATE shared pagination normalization.
- **IMPLEMENT**: Add a documented `MaxPageSize = 100` constant near the existing defaults. Make it accessible to tests, preferably `public const int MaxPageSize = 100;`, while keeping `DefaultPageSize` and `DefaultPageNumber` private unless tests need them. Update the `PageSize` setter so `value < 1` returns `DefaultPageSize`, `value > MaxPageSize` returns `MaxPageSize`, and all other positive values are preserved.
- **MIRROR**: `server/FinancesControl.API/Common/PaginationParams.cs:5-20` - keep backing fields and property setter normalization style.
- **IMPORTS**: None.
- **GOTCHA**: ASP.NET Core model binding assigns public writable properties on complex query-bound types, so this setter is the enforcement point; do not move enforcement to controllers or repositories.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 2: CREATE `server/FinancesControl.Tests/Common/PaginationParamsTests.cs`

- **ACTION**: CREATE focused unit tests for `PaginationParams`.
- **IMPLEMENT**: Add tests in namespace `FinancesControl.Tests.Common` using xUnit and FluentAssertions. Cover: default constructor returns `PageSize = 10` and `PageNumber = 1`; negative and zero `PageSize` use default `10`; a normal value such as `50` is preserved; `PageSize = PaginationParams.MaxPageSize` is preserved; `PageSize = PaginationParams.MaxPageSize + 1` clamps to `MaxPageSize`; negative and zero `PageNumber` use default `1`; `GetOffset()` uses clamped page size, e.g. `PageNumber = 2` and oversized `PageSize` returns offset `100`.
- **MIRROR**: `server/FinancesControl.Tests/Common/GlobalExceptionHandlerMiddlewareTests.cs:9-30` - Common test class and xUnit style; `server/FinancesControl.Tests/Accounts/Services/AccountServiceTests.cs:326-340` - FluentAssertions pagination assertions.
- **IMPORTS**: `using FluentAssertions;` and `using FinancesControl.API.Common;`.
- **PATTERN**: Use `[Fact]` for default and offset-specific behavior; use `[Theory]`/`[InlineData]` for equivalent page size normalization cases.
- **GOTCHA**: The test project targets `net8.0` in `server/FinancesControl.Tests/FinancesControl.Tests.csproj`, despite `docs/backend-testing-guidelines.md` saying `.NET 9.0`; follow the project file and solution commands.
- **VALIDATE**: `dotnet test server/FinancesControl.sln --filter FullyQualifiedName~PaginationParamsTests`

### `[x]` Task 3: VERIFY list-endpoint integration remains centralized

- **ACTION**: INSPECT/VERIFY no endpoint-specific clamp was added.
- **IMPLEMENT**: Confirm accounts, banks, cards, transaction categories, and transactions continue to pass the shared `PaginationParams` object through controller -> service -> repository, and that SQL command classes still read `paginationParams.PageSize` and `paginationParams.GetOffset()`.
- **MIRROR**: `server/FinancesControl.API/Modules/Accounts/Controllers/AccountController.cs:56-61`, `server/FinancesControl.API/Modules/Accounts/Services/AccountService.cs:67-86`, `server/FinancesControl.API/Modules/Accounts/Repository/SqlCommands/GetAllAccountsQuery.cs:20-21`.
- **IMPORTS**: None.
- **GOTCHA**: Avoid redundant clamps in five separate endpoints; duplicated logic would make future changes harder and could drift.
- **VALIDATE**: `dotnet test server/FinancesControl.sln`

---

## Testing Strategy

### Unit Tests to Write

| Test File                                | Test Cases                 | Validates      |
| ---------------------------------------- | -------------------------- | -------------- |
| `server/FinancesControl.Tests/Common/PaginationParamsTests.cs` | default constructor; negative page size; zero page size; normal positive page size; exact max page size; oversized page size; negative/zero page number; offset with clamped page size | Shared pagination normalization contract |

### Edge Cases Checklist

- [ ] Default `new PaginationParams()` remains `PageSize = 10`, `PageNumber = 1`.
- [ ] `PageSize < 0` normalizes to `10`.
- [ ] `PageSize = 0` normalizes to `10`.
- [ ] `PageSize = 1` remains `1`.
- [ ] `PageSize = 100` remains `100`.
- [ ] `PageSize > 100` clamps to `100`.
- [ ] `PageNumber < 1` still normalizes to `1`.
- [ ] `GetOffset()` uses the normalized and clamped values.

---

## Validation Commands

**Validation loop:** the plan is not complete until every command below passes (exit 0). On any failure, fix the cause and re-run -- loop until all pass. If a check is genuinely impossible, mark it `[f]`, note why in Agent Notes, and move on.

### Level 1: STATIC_ANALYSIS

```bash
dotnet build server/FinancesControl.sln
```

**EXPECT**: Exit 0, no compiler errors.

### Level 2: UNIT_TESTS

```bash
dotnet test server/FinancesControl.sln --filter FullyQualifiedName~PaginationParamsTests
```

**EXPECT**: New pagination tests pass.

### Level 3: FULL_SUITE

```bash
dotnet test server/FinancesControl.sln
```

**EXPECT**: All backend tests pass.

### Level 4: DATABASE_VALIDATION (if schema changes)

Not applicable. This plan does not change database schema or SQL text.

### Level 5: BROWSER_VALIDATION (if UI changes)

Not applicable. This plan does not change frontend UI.

### Level 6: MANUAL_VALIDATION

Manually inspect `PaginationParams.cs` and verify:

- `MaxPageSize` is documented in code and set to `100`.
- Negative/zero `PageSize` still defaults to `10`.
- Positive oversized `PageSize` clamps to `100`.
- `GetOffset()` still derives from `PageNumber` and normalized `PageSize`.

---

## Acceptance Criteria

- [ ] `PaginationParams` defines a documented maximum page size of `100`.
- [ ] `PageSize` values below `1` continue to normalize to the default page size `10`.
- [ ] `PageSize` values from `1` through `100` are preserved.
- [ ] `PageSize` values above `100` clamp to `100`.
- [ ] `GetOffset()` uses the normalized/clamped page size.
- [ ] Focused tests exist for negative, zero, default, normal, max, and oversized page sizes.
- [ ] `dotnet build server/FinancesControl.sln` passes.
- [ ] `dotnet test server/FinancesControl.sln` passes.

---

## Completion Checklist

- [ ] All tasks completed in dependency order.
- [ ] Each task validated immediately after completion.
- [ ] Level 1: Static analysis/build passes.
- [ ] Level 2: Focused unit tests pass.
- [ ] Level 3: Full backend test suite passes.
- [ ] Level 4: Database validation not applicable.
- [ ] Level 5: Browser validation not applicable.
- [ ] All acceptance criteria met.

---

## Risks and Mitigations

| Risk               | Likelihood   | Impact       | Mitigation                              |
| ------------------ | ------------ | ------------ | --------------------------------------- |
| Existing clients rely on `PageSize > 100` returning more than 100 records. | MED | MED | Clamp silently to preserve a successful response while bounding load; document max in `PaginationParams`. |
| Tests duplicate service-level pagination assertions across modules. | LOW | LOW | Add direct Common tests for `PaginationParams` instead of touching every service test file. |
| Offset expectations become confusing when page size is clamped. | LOW | MED | Include an explicit test proving `GetOffset()` uses the clamped value. |

---

## Questionables

<details>
<summary>Should `MaxPageSize` be public or private?</summary>

Assumption: make `MaxPageSize` a `public const int` so the documented limit can be referenced by tests without duplicating the magic number. This is still a narrow API surface on a shared common type and avoids appsettings/config complexity that is not documented yet.

</details>

---

## Agent Notes

APPROACH_CHOSEN: Central clamp inside `PaginationParams.PageSize`.

RATIONALE: The codebase already centralizes pagination defaults in `server/FinancesControl.API/Common/PaginationParams.cs`. All five affected list endpoints bind `[FromQuery] PaginationParams`, pass that object through services and repositories, and SQL command classes use `paginationParams.PageSize` for `LIMIT @PageSize`. One shared setter change protects every endpoint and keeps current architecture intact.

ALTERNATIVES_REJECTED:

- Add validation attributes and reject oversized requests with HTTP 400: rejected because existing behavior normalizes invalid pagination values rather than rejecting requests.
- Clamp in every controller action: rejected because it duplicates logic across accounts, banks, cards, categories, and transactions.
- Clamp in every SQL command class: rejected because pagination metadata in services would still see the unbounded value, making response metadata inconsistent with SQL results.
- Make the max configurable through appsettings: rejected because this task asks for a documented maximum and config conventions for this setting are not documented yet.

External research:

- ASP.NET Core model binding documentation confirms complex types need public writable properties and model binding assigns those properties. That supports central setter enforcement for `[FromQuery] PaginationParams`.
- ASP.NET Core action return type documentation confirms existing `ActionResult<PaginatedResponse<T>>` endpoints do not need response-shape changes for a normalization-only fix.

Dependency notes:

- `server/FinancesControl.Tests/FinancesControl.Tests.csproj` targets `net8.0`.
- Test packages include xUnit `2.9.2`, FluentAssertions `4.19.4`, NSubstitute `5.3.0`, Microsoft.NET.Test.Sdk `17.12.0`, coverlet.collector `6.0.2`.
- `docs/backend-testing-guidelines.md` says `.NET 9.0`, but the actual project file is authoritative for build/test execution.

Confidence score: 9/10. The integration point is centralized and already tested for invalid lower bounds; the only meaningful uncertainty is whether the implementation team prefers `MaxPageSize` public for tests or private with duplicated expected value in tests.

---

## Amendments

_Append-only history of changes made **after** this plan was first built (newest at the bottom). The build and update steps add entries here; never edit or remove existing ones._

<details>
<summary>2026-07-18T16:57:20.0578704-03:00 -- Initial plan</summary>

Created the implementation plan from `PRPs/tasks/limit-pagination-page-size.md`.

</details>

<details>
<summary>2026-07-18T17:09:22.4970875-03:00 -- Implementation complete</summary>

Implemented `PaginationParams.MaxPageSize = 100`, clamped positive oversized `PageSize` values, added focused common tests, verified list endpoint integration remains centralized, and validated with `dotnet build server/FinancesControl.sln`, filtered pagination tests, and `dotnet test server/FinancesControl.sln`.

</details>
