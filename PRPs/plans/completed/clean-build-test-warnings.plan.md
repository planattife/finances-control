# Feature: Clean Build/Test Warnings Before Release

## Summary

Clean the backend warning surface for release by making response payload nullability explicit, initializing or converting transaction DTOs so their contracts match nullable analysis, awaiting async substitute verification in tests, and adding a warnings-as-errors validation path after the warning cleanup is complete. The current workspace build and test commands pass with 0 warnings and 356 passing tests, but this plan preserves the requested release-hardening work and gives the implementation agent concrete audit steps for the warning clusters identified by codebase exploration.

## User Story

As a maintainer preparing the backend for release
I want build and test warning output to be clean and enforceable
So that CI failures point to real defects instead of noise hidden among nullable and async warnings

## Problem Statement

Nullable reference types are enabled in both backend projects, but several response and transaction DTO contracts can express nullable state more accurately, and async substitute calls in tests are mixed between awaited and unawaited forms. A release build/test workflow should finish with zero warnings and should be able to run with warnings treated as errors.

## Solution Statement

Audit the API response wrappers, transaction response DTOs, and backend tests under `server/FinancesControl.Tests`; update only warning-producing contracts or call sites; then validate with normal build/test and `-warnaserror` command-line checks. If the codebase remains warning-free, add project-level or CI-level warnings-as-errors only after confirming no legitimate warnings remain.

## Metadata

| Field            | Value                                                                 |
| ---------------- | --------------------------------------------------------------------- |
| Type             | REFACTOR                                                              |
| Complexity       | MEDIUM                                                                |
| Systems Affected | Backend API response DTOs, transaction DTOs, backend tests, build config |
| Dependencies     | .NET 8, xUnit 2.9.2, FluentAssertions 4.19.4, NSubstitute 5.3.0       |
| Estimated Tasks  | 6                                                                     |

---

## Lifecycle (append-only)

- **Created:** 2026-07-19T00:00:00-03:00
- **Modified:**
  - 2026-07-19T00:00:00-03:00 - Initial plan created from `PRPs/tasks/clean-build-test-warnings.md`
  - 2026-07-19T00:00:00-03:00 - Implemented warning cleanup and validated warnings-as-errors gates
- **Commits:** none yet
- **Agent / Session:**
  - Codex GPT-5 planning session, 2026-07-19
  - Codex GPT-5 implementation session, 2026-07-19
- **Back refs:**
  - `PRPs/tasks/clean-build-test-warnings.md` - source task
- **Forward refs:** none yet

> **Append-only:** `Created` is set once; every other field is a list you only ever add to - never overwrite or remove existing entries. Keep references bidirectional: when you add a back/forward ref here, add the reciprocal ref on the other plan.

---

## UX Design

### Before State

```
BEFORE STATE

  Developer/CI        -> dotnet build/test       -> warnings may blend into passing output
  API response DTOs   -> nullable analysis       -> Data and strings can be unclear
  Test substitutes    -> async Received calls    -> some Task-returning calls unawaited

  USER_FLOW: Maintainer runs backend build/test before release and must manually trust warning output.
  PAIN_POINT: Nullable and async warning noise can hide defects and weakens CI signal.
  DATA_FLOW: DTO/service/test code -> C# compiler analysis -> passing build/test output.
```

### After State

```
AFTER STATE

  Developer/CI        -> dotnet build/test       -> zero warnings confirmed
          |
          v
  Nullable contracts  -> explicit DTO payloads   -> compiler state matches runtime behavior
          |
          v
  Async test calls    -> awaited verification    -> no CS4014-style blind spots
          |
          v
  Warnings gate       -> -warnaserror or config  -> release build fails on warning regressions

  USER_FLOW: Maintainer runs validation and gets a clean, enforceable backend signal.
  VALUE_ADD: Release readiness improves because warnings become actionable defects.
  DATA_FLOW: DTO/service/test code -> nullable/async compiler analysis -> warning-free build/test gate.
```

### Interaction Changes

| Location | Before | After | User Impact |
|----------|--------|-------|-------------|
| `server/FinancesControl.API/Common/Responses` | Response payload nullability can be ambiguous | `SingleResponse<T>` and `PaginatedResponse<T>` express actual null/empty behavior | API maintainers can reason about not-found and empty-list responses from types |
| `server/FinancesControl.API/Modules/Transactions/DTOs` | Some mutable DTO strings are non-nullable without defaults | Strings are initialized or DTOs converted to records where practical | DTO warnings stop obscuring actual transaction mapping problems |
| `server/FinancesControl.Tests` | Async substitutes are verified inconsistently | Task-returning substitute calls are awaited or intentionally handled | Test failures and compiler warnings become more trustworthy |
| Build/CI command | Passing build may not fail on future warnings | Release validation includes warnings-as-errors once cleanup is done | Regressions fail quickly before release |

---

## Mandatory Reading

**CRITICAL: Implementation agent MUST read these files before starting any task:**

| Priority | File | Lines | Why Read This |
|----------|------|-------|---------------|
| P0 | `PRPs/tasks/clean-build-test-warnings.md` | all | Source scope and expected direction |
| P0 | `server/FinancesControl.API/Common/Responses/SingleResponse.cs` | 3-6 | Mutable single-item response wrapper whose `Data` contract drives many warnings |
| P0 | `server/FinancesControl.API/Common/Responses/PaginatedResponse.cs` | 3-20 | Paginated response wrapper and default list behavior |
| P0 | `server/FinancesControl.API/Modules/Transactions/DTOs/TransactionDetailResponse.cs` | 3-16 | Transaction detail DTO with nullable and non-nullable response fields |
| P0 | `server/FinancesControl.API/Modules/Transactions/DTOs/TransactionListItemResponse.cs` | 5-14 | Transaction list DTO with non-nullable mutable strings |
| P0 | `server/FinancesControl.API/Modules/Transactions/Services/TransactionService.cs` | 124-220 | Service mapping from domain transactions into response DTOs |
| P1 | `server/FinancesControl.Tests/Transactions/Services/TransactionServiceTests.cs` | 31-58, 103-105, 346-348 | Test setup, awaited substitute pattern, nullable assertion pattern |
| P1 | `server/FinancesControl.Tests/Transactions/Controllers/TransactionControllerTests.cs` | 150, 215 | Existing null response assertions and awaited async substitute verification |
| P1 | `server/FinancesControl.API/FinancesControl.API.csproj` | 3-7 | Nullable configuration for API project |
| P1 | `server/FinancesControl.Tests/FinancesControl.Tests.csproj` | 3-18 | Nullable configuration and test dependency versions |
| P2 | `docs/dapper-usage-guidelines.md` | all | Backend repository conventions, only relevant if warning fixes touch repository mapping |
| P2 | `docs/database-table-guidelines.md` | all | Confirms no database changes are needed for this plan |

**External Documentation:**
| Source | Section | Why Needed |
|--------|---------|------------|
| [Microsoft Learn - Nullable reference warnings](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/nullable-warnings) | Nonnullable reference not initialized | Use compiler-supported fixes for CS8618-style warnings |
| [Microsoft Learn - Nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-reference-types) | Nullable context | Confirm `<Nullable>enable</Nullable>` behavior already used by both projects |
| [Microsoft Learn - Async warnings / CS4014](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs4014) | Async practices | Await Task-returning substitute calls unless intentionally fire-and-forget |
| [Microsoft Learn - C# errors and warnings compiler options](https://learn.microsoft.com/sr-cyrl-rs/dotnet/csharp/language-reference/compiler-options/errors-warnings#treatwarningsaserrors) | TreatWarningsAsErrors | Decide whether to use command-line `-warnaserror` or project-level config |

---

## Patterns to Mirror

**NAMING_CONVENTION:**

```csharp
// SOURCE: server/FinancesControl.API/Modules/Banks/DTOs/BankDetailResponse.cs:5-12
// COPY THIS PATTERN:
public record BankDetailResponse(
    long Id,
    string Name,
    BankAccountType AccountType,
    string Status,
    DateTime CreatedAt,
    DateTime? LastUpdate
);
```

**ERROR_HANDLING:**

```csharp
// SOURCE: server/FinancesControl.API/Modules/Transactions/Services/TransactionService.cs:132-145
// COPY THIS PATTERN:
catch (ResourceNotFoundException ex)
{
    response.AddError(ex.Message);
    response.SetHttpStatusCode(404);
    return response;
}

if (t == null)
{
    response.AddError("Transaction not found.");
    response.SetHttpStatusCode(404);
    return response;
}
```

**LOGGING_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.Tests/Common/HttpsRedirectionTestingEnvironmentTests.cs:41-43
// COPY THIS PATTERN IF WARNING-RELATED LOG ASSERTIONS ARE TOUCHED:
builder.ConfigureLogging(logging =>
{
    logging.AddProvider(loggerProvider);
});
```

**REPOSITORY_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Modules/Transactions/Repository/ITransactionRepository.cs:9-14
// COPY THIS CONTRACT SHAPE IF REPOSITORY WARNING FIXES ARE REQUIRED:
Task<long> Add(Transaction transaction);
Task<Transaction?> GetById(long transactionId, long accountId);
Task<List<Transaction>> GetAll(long accountId, PaginationParams paginationParams);
Task<int> CountByAccountId(long accountId);
Task<bool> Update(Transaction transaction);
Task<bool> ExistsAsync(long transactionId, long accountId);
```

**SERVICE_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Modules/Transactions/Services/TransactionService.cs:147-163
// COPY THIS MAPPING STYLE:
var detail = new TransactionDetailResponse
{
    Id = t.Id,
    Amount = t.Amount,
    PaymentMethod = t.PaymentMethod.ToString(),
    TransactionType = t.TransactionType.ToString(),
    TransactionDate = t.TransactionDate,
    Category = new TransactionCategoryResponse
    {
        CategoryId = t.CategoryId,
        CategoryName = t.CategoryName
    },
    Description = t.Description,
    Status = t.IsActive ? "Active" : "Inactive",
    CreatedAt = t.CreatedAt,
    UpdatedAt = t.UpdatedAt ?? null
};
```

**TEST_STRUCTURE:**

```csharp
// SOURCE: server/FinancesControl.Tests/Transactions/Services/TransactionServiceTests.cs:101-105
// COPY THIS ASYNC SUBSTITUTE VERIFICATION STYLE:
var ex = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.CreateTransaction(request, AccountId, TestUserId));

ex.Message.Should().Contain($"Transaction category with ID {otherUserCategoryId} does not exist");
await _repository.DidNotReceive().Add(Arg.Any<Transaction>());
```

---

## Files to Change

| File | Action | Justification |
| ---- | ------ | ------------- |
| `server/FinancesControl.API/Common/Responses/SingleResponse.cs` | UPDATE | Make `Data` nullable if not-found responses intentionally carry no payload |
| `server/FinancesControl.API/Common/Responses/PaginatedResponse.cs` | UPDATE | Initialize `Data` to an empty list so paginated responses are never null |
| `server/FinancesControl.API/Modules/Transactions/DTOs/TransactionDetailResponse.cs` | UPDATE | Initialize non-nullable strings or convert to a record while preserving nullable `Card`, `Bank`, `Description`, `UpdatedAt` |
| `server/FinancesControl.API/Modules/Transactions/DTOs/TransactionListItemResponse.cs` | UPDATE | Initialize non-nullable strings or convert to a record to remove DTO construction warnings |
| `server/FinancesControl.Tests/**/*.cs` | UPDATE | Await Task-returning substitute verification and update null assertions after `SingleResponse<T>.Data` becomes nullable |
| `server/FinancesControl.API/FinancesControl.API.csproj` | UPDATE IF CHOSEN | Add warning enforcement only after normal build/test are warning-free |
| `server/FinancesControl.Tests/FinancesControl.Tests.csproj` | UPDATE IF CHOSEN | Add warning enforcement only after normal build/test are warning-free |

---

## NOT Building (Scope Limits)

Explicit exclusions to prevent scope creep:

- No database schema or SQL migration changes; warning cleanup is in DTO/test/build configuration.
- No frontend changes; the task is backend build/test release signal only.
- No broad DTO redesign outside warning-producing response and transaction DTO contracts.
- No dependency upgrades; use current .NET 8, xUnit, FluentAssertions, and NSubstitute versions.

---

## Step-by-Step Tasks

Execute in order. Each task is atomic and independently verifiable.

**Status markers** - prefix EVERY task header with one; the build agent updates it inline as it works: `[ ]` idle | `[wip]` in progress | `[x]` complete | `[f]` failed. All tasks start `[ ]`. If a task cannot be made to pass, mark it `[f]`, record why in Agent Notes, and move on if the rest of the plan can still proceed.

### `[x]` Task 1: AUDIT `server/FinancesControl.sln`

- **ACTION**: RUN baseline diagnostics.
- **IMPLEMENT**: Run `dotnet build server/FinancesControl.sln` and capture every warning code/file/line if any appear. If the workspace still reports 0 warnings, proceed with code audit tasks and preserve `-warnaserror` as the enforcement check.
- **PATTERN**: `server/FinancesControl.API/FinancesControl.API.csproj:5` and `server/FinancesControl.Tests/FinancesControl.Tests.csproj:6` already have `<Nullable>enable</Nullable>`.
- **GOTCHA**: This planning session observed `dotnet build server/FinancesControl.sln` with `0 Aviso(s)` and `dotnet test server/FinancesControl.sln` with 356 passing tests, while one subagent reported an older/separate 173-warning build. Trust the current command output in the implementation workspace.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 2: UPDATE `server/FinancesControl.API/Common/Responses/SingleResponse.cs`

- **ACTION**: UPDATE nullable payload contract if the baseline warning list or code audit confirms `Data` is unset/null in failure paths.
- **IMPLEMENT**: Change `public T Data { get; set; }` to `public T? Data { get; set; }`, preserving mutable response style and `BaseResponse` inheritance.
- **MIRROR**: `server/FinancesControl.API/Modules/Transactions/Services/TransactionService.cs:126-145` - read flows create an empty response and return errors without setting `Data`.
- **GOTCHA**: Do not use `null!` here; Microsoft nullable guidance favors expressing the actual nullable contract when null is a valid response state.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 3: UPDATE `server/FinancesControl.API/Common/Responses/PaginatedResponse.cs`

- **ACTION**: UPDATE list payload initialization.
- **IMPLEMENT**: Initialize `Data` as `new List<T>()` or `new()` so empty paginated responses are represented as an empty collection, not a nullable payload.
- **MIRROR**: `server/FinancesControl.API/Common/Responses/BaseResponse.cs:5` - collection properties are initialized at declaration.
- **GOTCHA**: Do not make `Data` nullable unless an existing API contract intentionally returns null for list endpoints; service methods already assign lists on success and empty lists are safer for consumers.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 4: UPDATE transaction response DTOs in `server/FinancesControl.API/Modules/Transactions/DTOs`

- **ACTION**: UPDATE `TransactionDetailResponse.cs` and `TransactionListItemResponse.cs`.
- **IMPLEMENT**: Either initialize mutable string properties with `string.Empty` or convert the two DTOs to records if that does not create a larger mapping rewrite. Keep `Card`, `Bank`, `Description`, and `UpdatedAt` nullable because they are genuinely optional.
- **MIRROR**: `server/FinancesControl.API/Modules/Transactions/DTOs/TransactionCategoryResponse.cs:5-6` and `server/FinancesControl.API/Modules/Transactions/DTOs/TransactionCardResponse.cs:5-6` for initialized mutable DTO strings; `server/FinancesControl.API/Modules/Banks/DTOs/BankDetailResponse.cs:5-12` for record DTO style.
- **GOTCHA**: If converting to records, update `TransactionService.cs:147-180` and `TransactionService.cs:203-220` object initialization carefully; keeping mutable classes with defaults is lower-risk for this cleanup.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 5: UPDATE async and nullable assertions in `server/FinancesControl.Tests/**/*.cs`

- **ACTION**: UPDATE warning-producing test call sites only.
- **IMPLEMENT**: Await all Task-returning substitute verifications, including `Received()` and `DidNotReceive()` calls against async service/repository methods. After `SingleResponse<T>.Data` becomes nullable, update assertions to use null checks plus `!` or local variables after the assertion.
- **MIRROR**: `server/FinancesControl.Tests/Transactions/Services/TransactionServiceTests.cs:103-105` for awaited `DidNotReceive`; `server/FinancesControl.Tests/Accounts/Controllers/AccountControllerTests.cs:42-43` for null-forgiving after a type assertion.
- **GOTCHA**: Microsoft CS4014 guidance says unawaited Task-returning calls can continue before completion and hide exceptions. Do not blanket-disable CS4014 with pragmas for this release cleanup.
- **VALIDATE**: `dotnet test server/FinancesControl.sln`

### `[x]` Task 6: ADD warning enforcement in backend project config or CI command

- **ACTION**: UPDATE project files if the team wants permanent local enforcement; otherwise document/use command-line enforcement for CI.
- **IMPLEMENT**: Preferred low-risk release gate: make CI run `dotnet build server/FinancesControl.sln -warnaserror` and `dotnet test server/FinancesControl.sln -warnaserror`. If project-level enforcement is desired, add `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` to both backend project files only after Tasks 1-5 are warning-free.
- **MIRROR**: `server/FinancesControl.API/FinancesControl.API.csproj:3-7` and `server/FinancesControl.Tests/FinancesControl.Tests.csproj:3-8` for existing property group style.
- **GOTCHA**: Microsoft documents that project-level `<TreatWarningsAsErrors>` affects the C# compiler, while command-line `-warnaserror` affects all tasks. Choose intentionally.
- **VALIDATE**: `dotnet build server/FinancesControl.sln -warnaserror` and `dotnet test server/FinancesControl.sln -warnaserror`

---

## Testing Strategy

### Unit Tests to Write

| Test File | Test Cases | Validates |
| --------- | ---------- | --------- |
| No new test file expected | Existing response/controller/service tests should continue passing after contract cleanup | No behavioral regressions |
| `server/FinancesControl.Tests/Transactions/Services/TransactionServiceTests.cs` | Existing not-found, nullable card/bank, and list response assertions | Nullable response contracts remain correct |
| `server/FinancesControl.Tests/Transactions/Controllers/TransactionControllerTests.cs` | Existing response status and service verification assertions | Controllers still return expected wrapper types |

### Edge Cases Checklist

- [ ] `SingleResponse<T>` not-found paths return errors and no payload without nullable warnings.
- [ ] `PaginatedResponse<T>` empty-list paths return `Data` as an empty list, not null.
- [ ] Cash transactions have no `Card` or `Bank` response payload.
- [ ] Card transactions have `Card` payload and no accidental bank payload.
- [ ] Bank/Pix/BankSlip/BankTransfer transactions have `Bank` payload and no accidental card payload.
- [ ] Async substitute verification calls compile without CS4014 and still assert call counts.
- [ ] Null `InlineData` theory parameters are typed as nullable where needed.

---

## Validation Commands

**IMPORTANT**: Replace these placeholders with actual commands from the project's package.json/config.

Validation loop: the plan is not complete until every command below passes (exit 0). On any failure, fix the cause and re-run; loop until all pass. If a check is genuinely impossible, mark it `[f]`, note why in Agent Notes, and move on.

### Level 1: STATIC_ANALYSIS

```bash
dotnet build server/FinancesControl.sln -warnaserror
```

**EXPECT**: Exit 0, no errors or warnings

### Level 2: UNIT_TESTS

```bash
dotnet test server/FinancesControl.sln -warnaserror
```

**EXPECT**: All backend tests pass; this planning session observed 356 passing tests

### Level 3: FULL_SUITE

```bash
dotnet test server/FinancesControl.sln -warnaserror
dotnet build server/FinancesControl.sln -warnaserror
```

**EXPECT**: All tests pass, build succeeds, warning regressions fail the command

### Level 4: DATABASE_VALIDATION (if schema changes)

Not applicable. This plan should not change database schema or SQL scripts.

### Level 5: BROWSER_VALIDATION (if UI changes)

Not applicable. This plan has no frontend or browser-facing changes.

### Level 6: MANUAL_VALIDATION

1. Inspect final build output and confirm `0 Warning(s)` or localized equivalent.
2. Inspect final test output and confirm all tests pass.
3. Search for unawaited async substitute verification patterns with `rg -n "Received\\(|DidNotReceive\\(" server/FinancesControl.Tests` and verify Task-returning calls are awaited.
4. Search for nullable suppressions with `rg -n "null!|#pragma warning disable 4014|= null;" server/FinancesControl.API server/FinancesControl.Tests` and ensure each remaining use is intentional.

---

## Acceptance Criteria

- [ ] `dotnet build server/FinancesControl.sln -warnaserror` exits 0.
- [ ] `dotnet test server/FinancesControl.sln -warnaserror` exits 0.
- [ ] `SingleResponse<T>` payload nullability matches not-found/error response behavior.
- [ ] `PaginatedResponse<T>` always has a non-null list payload.
- [ ] Transaction DTOs no longer depend on uninitialized non-nullable strings.
- [ ] Async substitute verification calls in tests are awaited or otherwise intentionally handled without suppressing release warnings.
- [ ] No database, frontend, or dependency-upgrade changes are introduced.

---

## Completion Checklist

- [ ] All tasks completed in dependency order
- [ ] Each task validated immediately after completion
- [ ] Level 1: Static analysis passes with warnings as errors
- [ ] Level 2: Unit tests pass with warnings as errors
- [ ] Level 3: Full test suite + build succeeds
- [ ] Level 4: Database validation marked not applicable
- [ ] Level 5: Browser validation marked not applicable
- [ ] All acceptance criteria met

---

## Risks and Mitigations

| Risk | Likelihood | Impact | Mitigation |
| ---- | ---------- | ------ | ---------- |
| Making `SingleResponse<T>.Data` nullable produces many test nullable warnings | MED | MED | Update assertions immediately after response contract change; use null checks and local variables |
| Converting transaction DTO classes to records expands the mapping diff | MED | MED | Prefer property defaults unless records clearly reduce warnings with minimal service changes |
| Warnings-as-errors exposes non-C# task warnings in CI | LOW | MED | Start with command-line `-warnaserror` in validation; add project-level config only after the gate is stable |
| Current workspace has 0 warnings while task mentions many warnings | MED | LOW | Treat Task 1 baseline as authoritative; if no warnings remain, implement only enforcement and any still-ambiguous contracts |

---

## Questionables

<details>
<summary>Should warnings-as-errors be project-level or CI-only?</summary>

Assumption: use command-line `-warnaserror` as the release validation gate first, then add `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` to project files only if the team wants local builds to fail on warnings. This avoids surprising local workflows while still protecting release CI.

</details>

<details>
<summary>Should transaction response DTOs become records?</summary>

Assumption: keep mutable classes and add defaults unless the implementation agent finds actual warnings that records solve more cleanly. Other modules use record response DTOs, but current transaction service mapping already uses object initializers, so defaults are the smaller cleanup.

</details>

---

## Agent Notes

Plan input type: document file, `PRPs/tasks/clean-build-test-warnings.md`.

Feature type: REFACTOR. Complexity: MEDIUM because response wrapper nullability affects many service/controller/test call sites, but no new behavior or data model is being introduced.

Approach chosen: warning-driven cleanup with compiler validation after each cluster. The implementation should not modify every DTO preemptively; it should apply fixes where nullable analysis or explicit code audit shows a mismatch.

Alternatives rejected:

- Add `#pragma warning disable` around unawaited test verifications: rejected because the task goal is to clean release signal, not hide it.
- Add `null!` to response DTOs: rejected for response payloads where null is real not-found behavior.
- Convert all response DTOs in the API to records: rejected as out of scope and higher risk before release.

External research gotchas:

- Nullable `?` changes compiler intent, not runtime type; use it to express real optional payloads.
- CS4014 exists because unawaited Task-returning calls can hide completion and exception behavior; tests should await async substitutes where possible.
- Project-level `<TreatWarningsAsErrors>` and command-line `-warnaserror` are not identical; command-line is the stronger CI gate across build tasks.

Current observed validation during planning:

- `dotnet build server/FinancesControl.sln` exited 0 with `0 Aviso(s)`.
- `dotnet test server/FinancesControl.sln` exited 0 with 356 passed tests.

Subagent note: one explorer run reported an older/separate `173` warning count, while the current main workspace and analyst validation reported 0 warnings. The implementation agent should rerun Task 1 and treat that output as canonical.

Confidence score: 8/10. The plan is scoped and compiler-validated, with the only uncertainty being whether the current branch already cleaned the originally reported warnings.

---

## Amendments

<details>
<summary>2026-07-19T00:00:00-03:00 - Initial creation</summary>

Created from `PRPs/tasks/clean-build-test-warnings.md` using codebase exploration, project docs, current build/test validation, and Microsoft nullable/async warning guidance.

</details>

<details>
<summary>2026-07-19T00:00:00-03:00 - Implemented warning cleanup</summary>

Implemented nullable response contracts, transaction DTO defaults, awaited async substitute verifications, nullable test assertion fixes, xUnit nullable theory signatures, and warning-gate validation. Chose command-line `-warnaserror` enforcement instead of project-level `<TreatWarningsAsErrors>` to keep local build behavior unchanged while providing a release-ready validation gate.

</details>
