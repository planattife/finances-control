# Feature: Scope Transaction Category Ownership In Transaction Validation

## Summary

Transaction create and update validation must reject category IDs that are neither default categories nor custom categories owned by the authenticated user. The implementation should propagate `userId` through the transaction category existence contract and update the Dapper existence query to mirror the category read/list visibility predicate already used by the repository.

## User Story

As an authenticated finance app user
I want transaction category validation to accept only default categories or my own custom categories
So that another user's private category cannot be attached to my transactions by guessing an ID

## Problem Statement

`TransactionService` validates account, card, and bank ownership with the current user context, but category existence is checked globally. A transaction can currently be created or updated with another user's custom `TransactionCategories.Id` because `TransactionCategoryExistsQuery` only checks `Id`.

## Solution Statement

Change `ITransactionCategoryService.ExistsAsync`, `TransactionCategoryService.ExistsAsync`, `ITransactionCategoryRepository.ExistsAsync`, `TransactionCategoryRepository.ExistsAsync`, and `TransactionCategoryExistsQuery.Build` to accept `userId`. Update create/update validation in `TransactionService` to pass the authenticated user ID. The SQL predicate must become `WHERE "Id" = @CategoryId AND ("IsDefault" = true OR "UserId" = @UserId)`, matching the existing `GetById`, `GetAll`, and count visibility rules.

## Metadata

| Field            | Value |
| ---------------- | ----- |
| Type             | BUG_FIX |
| Complexity       | MEDIUM |
| Systems Affected | Backend transactions, transaction categories, Dapper SQL commands, backend unit tests |
| Dependencies     | Dapper 2.1.72, NSubstitute 5.3.0, xUnit 2.9.2, FluentAssertions 4.19.4 |
| Estimated Tasks  | 7 |

---

## Lifecycle (append-only)

- **Created:** 2026-07-15T17:29:00-03:00
- **Modified:**
  - 2026-07-15T17:29:00-03:00 - Initial implementation plan created from `PRPs/tasks/scope-transaction-category-ownership.md`.
  - 2026-07-15T18:18:55-03:00 - Implemented scoped transaction category ownership validation and completed backend validation.
- **Commits:** None yet.
- **Agent / Session:**
  - Codex GPT-5 session - plan creation.
  - Codex GPT-5 session - implementation.
- **Back refs:**
  - `PRPs/tasks/scope-transaction-category-ownership.md` - source task.
- **Forward refs:** None yet.

> **Append-only:** `Created` is set once; every other field is a list you only ever add to - never overwrite or remove existing entries. Keep references bidirectional: when you add a back/forward ref here, add the reciprocal ref on the other plan.

---

## UX Design

### Before State

```
+------------------------------------------------------------------------------+
|                                BEFORE STATE                                  |
+------------------------------------------------------------------------------+
|                                                                              |
|   [Transaction API] ----> [Category Exists?] ----> [Transaction Saved]        |
|                                                                              |
|   USER_FLOW: User submits create/update transaction with a category ID.       |
|   PAIN_POINT: Any existing category ID passes, even if it belongs to another  |
|   user's custom category.                                                    |
|   DATA_FLOW: Controller userId -> TransactionService -> ExistsAsync(id) ->    |
|   SQL WHERE "Id" = @CategoryId -> true -> transaction uses that category.     |
|                                                                              |
+------------------------------------------------------------------------------+
```

### After State

```
+------------------------------------------------------------------------------+
|                                 AFTER STATE                                  |
+------------------------------------------------------------------------------+
|                                                                              |
|   [Transaction API] ----> [Scoped Category Exists?] ----> [Save or 404]       |
|                                  |                                           |
|                                  v                                           |
|                     ["IsDefault" = true OR "UserId" = userId]               |
|                                                                              |
|   USER_FLOW: User submits create/update transaction with a category ID.       |
|   VALUE_ADD: Default categories and the user's own categories work; another   |
|   user's custom category is rejected as not found.                            |
|   DATA_FLOW: Controller userId -> TransactionService -> ExistsAsync(id,user)  |
|   -> scoped SQL predicate -> true/false -> save or ResourceNotFoundException. |
|                                                                              |
+------------------------------------------------------------------------------+
```

### Interaction Changes

| Location | Before | After | User Impact |
|----------|--------|-------|-------------|
| `POST /api/accounts/{accountId}/transactions` | Category validation checks global existence only. | Category validation checks default-or-current-user visibility. | Cross-user custom category IDs are rejected with the existing not-found behavior. |
| `PUT /api/accounts/{accountId}/transactions/{transactionId}` | Changing category can use any existing category ID. | Changing category can use only default or owned categories. | Users cannot accidentally or maliciously attach another user's category. |

---

## Mandatory Reading

**CRITICAL: Implementation agent MUST read these files before starting any task:**

| Priority | File | Lines | Why Read This |
|----------|------|-------|---------------|
| P0 | `docs/dapper-usage-guidelines.md` | 106-200 | SQL must remain in `SqlCommands`, repositories must stay thin, and all queries must be parameterized. |
| P0 | `server/FinancesControl.API/Modules/Transactions/Services/TransactionService.cs` | 71-113, 247-279 | Current create/update category validation call sites to change. |
| P0 | `server/FinancesControl.API/Modules/TransactionCategories/Repository/SqlCommands/GetTransactionCategoryByIdQuery.cs` | 7-18 | Exact visibility predicate to mirror for default-or-owner categories. |
| P0 | `server/FinancesControl.API/Modules/TransactionCategories/Repository/SqlCommands/TransactionCategoryExistsQuery.cs` | 7-18 | Current vulnerable global existence query. |
| P1 | `server/FinancesControl.API/Modules/TransactionCategories/Services/ITransactionCategoryService.cs` | 6-15 | Service contract to update. |
| P1 | `server/FinancesControl.API/Modules/TransactionCategories/Repository/ITransactionCategoryRepository.cs` | 6-14 | Repository contract to update. |
| P1 | `server/FinancesControl.API/Modules/TransactionCategories/Services/TransactionCategoryService.cs` | 103-106 | Service delegation to update. |
| P1 | `server/FinancesControl.API/Modules/TransactionCategories/Repository/TransactionCategoryRepository.cs` | 74-80 | Dapper query build/execution path to update. |
| P2 | `server/FinancesControl.Tests/Transactions/Services/TransactionServiceTests.cs` | 20-140, 520-617 | Existing NSubstitute setup, exception assertions, create/update tests. |
| P2 | `server/FinancesControl.Tests/TransactionCategories/Services/TransactionCategoryServiceTests.cs` | 1-35, 103-114 | Service test style and repository collaboration assertions. |

**External Documentation:**
| Source | Section | Why Needed |
|--------|---------|------------|
| [Dapper 2.1.72 parameters](https://www.learndapper.com/parameters#dapper-dynamic-parameters) | Dynamic Parameters | Confirms continuing to use parameterized SQL and `DynamicParameters.Add` for `@UserId`. |
| [NSubstitute 5.3 argument matchers](https://nsubstitute.github.io/help/argument-matchers/) | Argument matchers | Use `Arg.Any<T>()` / `Arg.Is<T>()` only in substitute setup or received checks when updating tests. |
| [xUnit 2.9 async assertions](https://api.xunit.net/v3/3.0.0/v3.3.0.0-Xunit.Assert.ThrowsAsync.html) | `Assert.ThrowsAsync` | Preserve current async exception assertion style for rejection tests. |

---

## Patterns to Mirror

**NAMING_CONVENTION:**

```csharp
// SOURCE: server/FinancesControl.API/Modules/Banks/Repository/SqlCommands/BankExistsQuery.cs:5-8
// COPY THIS PATTERN:
public class BankExistsQuery
{
    public (string Sql, DynamicParameters Params) Build(long bankId, string userId)
```

**ERROR_HANDLING:**

```csharp
// SOURCE: server/FinancesControl.API/Modules/Transactions/Services/TransactionService.cs:117-121
// COPY THIS PATTERN:
if (string.IsNullOrEmpty(userId))
    throw new AuthenticationContextException("User context is not available.");

if (!await _accountService.ExistsAsync(accountId, userId))
    throw new ResourceNotFoundException("Account not found.");
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

No service/repository-level logging exists in this feature area; do not add new logging for this fix.

**REPOSITORY_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Modules/TransactionCategories/Repository/TransactionCategoryRepository.cs:74-80
// COPY THIS PATTERN:
public async Task<bool> ExistsAsync(long categoryId)
{
    using var connection = _connectionFactory.CreateConnection();
    var query = new TransactionCategoryExistsQuery();
    var (sql, parameters) = query.Build(categoryId);
    
    return await connection.QuerySingleOrDefaultAsync<bool>(sql, parameters);
}
```

**SERVICE_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Modules/Transactions/Services/TransactionService.cs:77-82
// COPY THIS PATTERN:
if (request.CategoryId <= 0)
    throw new DomainValidationException("CategoryId must be greater than 0.");

if (!await _categoryService.ExistsAsync(request.CategoryId))
    throw new ResourceNotFoundException($"Transaction category with ID {request.CategoryId} does not exist.");
```

**TEST_STRUCTURE:**

```csharp
// SOURCE: server/FinancesControl.Tests/Transactions/Services/TransactionServiceTests.cs:83-90
// COPY THIS PATTERN:
public async Task CreateTransaction_WithNonexistentCategoryId_Throws()
{
    var request = new CreateTransactionRequest { Amount = 100, PaymentMethod = PaymentMethod.Cash, CategoryId = 999, TransactionType = TransactionType.Expense, TransactionDate = DateTime.UtcNow, AccountId = AccountId };
    SetupAccountExists();
    _categoryService.ExistsAsync(999).Returns(Task.FromResult(false));
    
    var ex = await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.CreateTransaction(request, AccountId, TestUserId));
    ex.Message.Should().Contain("Transaction category with ID 999 does not exist");
}
```

---

## Files to Change

| File | Action | Justification |
| ---- | ------ | ------------- |
| `server/FinancesControl.API/Modules/TransactionCategories/Services/ITransactionCategoryService.cs` | UPDATE | Add `userId` to the category existence service contract. |
| `server/FinancesControl.API/Modules/TransactionCategories/Services/TransactionCategoryService.cs` | UPDATE | Forward `userId` to the repository existence check. |
| `server/FinancesControl.API/Modules/TransactionCategories/Repository/ITransactionCategoryRepository.cs` | UPDATE | Add `userId` to the repository existence contract. |
| `server/FinancesControl.API/Modules/TransactionCategories/Repository/TransactionCategoryRepository.cs` | UPDATE | Build the existence query with category ID and user ID. |
| `server/FinancesControl.API/Modules/TransactionCategories/Repository/SqlCommands/TransactionCategoryExistsQuery.cs` | UPDATE | Scope existence by default category or current user ownership. |
| `server/FinancesControl.API/Modules/Transactions/Services/TransactionService.cs` | UPDATE | Pass `authenticatedUserId` into category validation for create and update. |
| `server/FinancesControl.Tests/Transactions/Services/TransactionServiceTests.cs` | UPDATE | Update mocks to the new signature and add cross-user rejection coverage for create/update. |
| `server/FinancesControl.Tests/TransactionCategories/Services/TransactionCategoryServiceTests.cs` | UPDATE | Add service delegation coverage for `ExistsAsync(categoryId, userId)`. |

---

## NOT Building (Scope Limits)

Explicit exclusions to prevent scope creep:

- No database migration: `UserId` and `IsDefault` already exist on `TransactionCategories`.
- No frontend changes: this is an API validation bug and preserves the existing not-found error shape.
- No endpoint or DTO changes: controllers already pass `userId` into transaction service methods.
- No transaction list/detail query changes: this task concerns validating category IDs during create/update, not filtering existing transaction reads.
- No new authorization framework: use the repository predicate pattern already present in transaction category queries.

---

## Step-by-Step Tasks

Execute in order. Each task is atomic and independently verifiable.

**Status markers** - prefix EVERY task header with one; the build agent updates it inline as it works: `[ ]` idle - `[wip]` in progress - `[x]` complete - `[f]` failed. All tasks start `[ ]`. If a task cannot be made to pass, mark it `[f]`, record why in Agent Notes, and move on if the rest of the plan can still proceed.

### `[x]` Task 1: UPDATE `server/FinancesControl.API/Modules/TransactionCategories/Repository/SqlCommands/TransactionCategoryExistsQuery.cs`

- **ACTION**: UPDATE query builder signature and SQL predicate.
- **IMPLEMENT**: Change `Build(long categoryId)` to `Build(long categoryId, string? userId)`. Add `AND ("IsDefault" = true OR "UserId" = @UserId)` inside the `SELECT EXISTS` query. Add `parameters.Add("@UserId", userId ?? (object)DBNull.Value);`.
- **MIRROR**: `server/FinancesControl.API/Modules/TransactionCategories/Repository/SqlCommands/GetTransactionCategoryByIdQuery.cs:7-18` - same nullable user parameter and default-or-owner predicate.
- **GOTCHA**: Keep the query parameterized; do not interpolate `userId` or `categoryId` into SQL.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 2: UPDATE `server/FinancesControl.API/Modules/TransactionCategories/Repository/ITransactionCategoryRepository.cs`

- **ACTION**: UPDATE interface contract.
- **IMPLEMENT**: Change `Task<bool> ExistsAsync(long categoryId);` to `Task<bool> ExistsAsync(long categoryId, string? userId);`.
- **MIRROR**: `server/FinancesControl.API/Modules/TransactionCategories/Repository/ITransactionCategoryRepository.cs:8-13` - keep simple repository method declarations.
- **GOTCHA**: Use nullable `string? userId` to align with existing category read methods.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 3: UPDATE `server/FinancesControl.API/Modules/TransactionCategories/Repository/TransactionCategoryRepository.cs`

- **ACTION**: UPDATE repository method signature and query invocation.
- **IMPLEMENT**: Change `ExistsAsync(long categoryId)` to `ExistsAsync(long categoryId, string? userId)` and call `query.Build(categoryId, userId)`.
- **MIRROR**: `server/FinancesControl.API/Modules/TransactionCategories/Repository/TransactionCategoryRepository.cs:30-36` - repository gets SQL/parameters from query class and executes via Dapper.
- **GOTCHA**: Keep SQL out of the repository; only pass the new argument to the SQL command class.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 4: UPDATE `server/FinancesControl.API/Modules/TransactionCategories/Services/ITransactionCategoryService.cs` and `server/FinancesControl.API/Modules/TransactionCategories/Services/TransactionCategoryService.cs`

- **ACTION**: UPDATE service contract and implementation.
- **IMPLEMENT**: Change service `ExistsAsync(long categoryId)` to `ExistsAsync(long categoryId, string? userId)` and delegate to `_repository.ExistsAsync(categoryId, userId)`.
- **MIRROR**: `server/FinancesControl.API/Modules/TransactionCategories/Services/TransactionCategoryService.cs:23-27` - service methods pass `userId` through to repository visibility checks.
- **GOTCHA**: Do not add authentication validation inside category service for this method; `TransactionService.ValidateAccount` already rejects missing user context before category validation.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 5: UPDATE `server/FinancesControl.API/Modules/Transactions/Services/TransactionService.cs`

- **ACTION**: UPDATE category existence call sites.
- **IMPLEMENT**: In `ValidateForeignKeys`, call `_categoryService.ExistsAsync(request.CategoryId, authenticatedUserId)`. In `Update`, after `ValidateAccount`, use `userId!` or an `authenticatedUserId` local and call `_categoryService.ExistsAsync(request.NewCategoryId.Value, authenticatedUserId)`.
- **MIRROR**: `server/FinancesControl.API/Modules/Transactions/Services/TransactionService.cs:90-111` - card and bank validation already pass `authenticatedUserId` into visibility checks.
- **GOTCHA**: Preserve the current exception messages so controller/global exception behavior and tests remain stable.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 6: UPDATE `server/FinancesControl.Tests/TransactionCategories/Services/TransactionCategoryServiceTests.cs`

- **ACTION**: ADD service delegation tests for scoped existence.
- **IMPLEMENT**: Add tests such as `ExistsAsync_PassesUserIdToRepository_ReturnsTrue` and `ExistsAsync_WhenRepositoryReturnsFalse_ReturnsFalse`. Stub `_repository.ExistsAsync(1, TestUserId)` and assert `await _repository.Received(1).ExistsAsync(1, TestUserId);`.
- **MIRROR**: `server/FinancesControl.Tests/TransactionCategories/Services/TransactionCategoryServiceTests.cs:103-114` - collaboration assertions use `Received(1)`.
- **GOTCHA**: Existing tests do not currently cover `ExistsAsync`; add focused tests instead of broad integration-style setup.
- **VALIDATE**: `dotnet test server/FinancesControl.sln --filter FullyQualifiedName~TransactionCategoryServiceTests`

### `[x]` Task 7: UPDATE `server/FinancesControl.Tests/Transactions/Services/TransactionServiceTests.cs`

- **ACTION**: UPDATE mocks and ADD cross-user rejection tests.
- **IMPLEMENT**: Replace category stubs from `_categoryService.ExistsAsync(categoryId)` to `_categoryService.ExistsAsync(categoryId, TestUserId)`. Add a create test that stubs `_categoryService.ExistsAsync(otherUserCategoryId, TestUserId)` as `false` and expects `ResourceNotFoundException`. Add an update test with existing transaction and `NewCategoryId` where scoped category existence returns false; assert the same not-found message and verify `_repository.Update` is not called.
- **MIRROR**: `server/FinancesControl.Tests/Transactions/Services/TransactionServiceTests.cs:83-90` for create exception style and `server/FinancesControl.Tests/Transactions/Services/TransactionServiceTests.cs:520-531` for update category setup.
- **GOTCHA**: There are many existing `_categoryService.ExistsAsync(...)` stubs; update all of them or the test project will not compile after the interface change.
- **VALIDATE**: `dotnet test server/FinancesControl.sln --filter FullyQualifiedName~TransactionServiceTests`

---

## Testing Strategy

### Unit Tests to Write

| Test File | Test Cases | Validates |
| --------- | ---------- | --------- |
| `server/FinancesControl.Tests/TransactionCategories/Services/TransactionCategoryServiceTests.cs` | `ExistsAsync_PassesUserIdToRepository_ReturnsTrue`, `ExistsAsync_WhenRepositoryReturnsFalse_ReturnsFalse` | New service/repository contract passes user context. |
| `server/FinancesControl.Tests/Transactions/Services/TransactionServiceTests.cs` | `CreateTransaction_WithOtherUsersCategoryId_Throws`, `Update_WithOtherUsersCategoryId_Throws` | Create/update reject categories not visible to current user. |
| `server/FinancesControl.Tests/Transactions/Services/TransactionServiceTests.cs` | Existing successful create/update tests with new `ExistsAsync(categoryId, TestUserId)` stubs | Existing default/owned happy paths still pass through validation. |

### Edge Cases Checklist

- [ ] Category ID <= 0 still throws `DomainValidationException` before service existence lookup.
- [ ] Missing user context still throws `AuthenticationContextException` via `ValidateAccount`.
- [ ] Default category returns true when `IsDefault = true`, regardless of `UserId`.
- [ ] Current user's custom category returns true when `UserId = @UserId`.
- [ ] Another user's custom category returns false and surfaces existing not-found behavior.
- [ ] Update path does not call repository update when new category is not visible.

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
dotnet test server/FinancesControl.sln --filter FullyQualifiedName~TransactionServiceTests
dotnet test server/FinancesControl.sln --filter FullyQualifiedName~TransactionCategoryServiceTests
```

**EXPECT**: All targeted service tests pass.

### Level 3: FULL_SUITE

```bash
dotnet test server/FinancesControl.sln
dotnet build server/FinancesControl.sln
```

**EXPECT**: Full backend test suite passes and solution builds.

### Level 4: DATABASE_VALIDATION (if schema changes)

No schema change expected. Validate SQL shape through code review of `TransactionCategoryExistsQuery`: it must include `("IsDefault" = true OR "UserId" = @UserId)` and bind both parameters.

### Level 5: BROWSER_VALIDATION (if UI changes)

Not applicable; no UI changes.

### Level 6: MANUAL_VALIDATION

1. With two users, create a custom transaction category for User A.
2. As User B, attempt to create a transaction using User A's custom category ID.
3. Confirm the API returns the existing transaction category not-found response.
4. As User B, create a transaction using a default category ID and confirm it succeeds.
5. As User B, create a custom category and confirm a transaction using that category succeeds.
6. Repeat the cross-user category check on transaction update with `NewCategoryId`.

---

## Acceptance Criteria

- [ ] `TransactionCategoryExistsQuery` returns true only for matching category ID plus default-or-owner visibility.
- [ ] `TransactionService.CreateTransaction` passes current user ID into category existence validation.
- [ ] `TransactionService.Update` passes current user ID into category existence validation.
- [ ] All compile errors from changed service/repository interface signatures are resolved.
- [ ] Unit tests cover cross-user category rejection on create and update.
- [ ] Level 1-3 validation commands pass with exit 0.
- [ ] No frontend, endpoint, DTO, or database migration changes are introduced.
- [ ] UX matches "After State" diagram.

---

## Completion Checklist

- [ ] All tasks completed in dependency order
- [ ] Each task validated immediately after completion
- [ ] Level 1: Static analysis (build) passes
- [ ] Level 2: Unit tests pass
- [ ] Level 3: Full test suite + build succeeds
- [ ] Level 4: Database validation passes (if applicable)
- [ ] Level 5: Browser validation passes (if applicable)
- [ ] All acceptance criteria met

---

## Risks and Mitigations

| Risk | Likelihood | Impact | Mitigation |
| ---- | ---------- | ------ | ---------- |
| Missing one of the many transaction test stubs after changing the interface | MED | MED | Use `rg -n "ExistsAsync\\(" server/FinancesControl.Tests server/FinancesControl.API` after edits and update every category-service call. |
| Accidentally excluding default categories with nullable `UserId` | LOW | HIGH | Mirror `GetTransactionCategoryByIdQuery` exactly: `("IsDefault" = true OR "UserId" = @UserId)`. |
| Changing error behavior in transaction flows | LOW | MED | Preserve existing `ResourceNotFoundException` messages and existing `DomainValidationException` ordering. |
| Adding SQL directly in repository | LOW | MED | Keep SQL isolated in `TransactionCategoryExistsQuery`, per `docs/dapper-usage-guidelines.md`. |

---

## Questionables

<details>
<summary>Should `ExistsAsync` require non-null `string userId` instead of `string? userId`?</summary>

Assumption: use `string? userId` because existing category read methods (`GetById`, `GetAll`, `CountByUserId`) already accept nullable user IDs and convert null to `DBNull.Value`. `TransactionService` still validates authenticated context before calling category existence, so this does not weaken the transaction path.

</details>

<details>
<summary>Should inactive categories be rejected?</summary>

Assumption: no. Existing `GetById`, `GetAll`, and `TransactionCategoryExistsQuery` do not filter by `IsActive`; this task is specifically about ownership/default visibility. Changing active-status behavior would be a separate business rule.

</details>

---

## Agent Notes

APPROACH_CHOSEN: Propagate `userId` through the existing category existence contract and update the existing SQL command with the same default-or-owner predicate used by category reads.

RATIONALE: This is the smallest architecture-faithful fix. The project already models category visibility with nullable `UserId` and `IsDefault`, and Dapper SQL is organized in module-level query classes. The transaction service already has user context after `ValidateAccount`, so no controller or DTO changes are needed.

ALTERNATIVES_REJECTED:

- Replace `ExistsAsync` with `GetById(categoryId, userId) != null`: rejected because it would fetch unnecessary columns and bypass the established existence-query pattern used by accounts, banks, cards, and transactions.
- Enforce this in controller authorization filters: rejected because the validation belongs with transaction foreign-key/business validation and must also apply consistently to create/update service calls.
- Add a database constraint: rejected because default-or-owner visibility depends on current authenticated user context, which cannot be represented as a static foreign key constraint.

Discovery highlights:

| Category | File:Lines | Pattern Description | Code Snippet |
| -------- | ---------- | ------------------- | ------------ |
| FLOW | `server/FinancesControl.API/Modules/Transactions/Controllers/TransactionController.cs:22-60` | Controller extracts `userId` and passes it to transaction service create/update. | `var userId = GetUserId();` |
| SERVICE | `server/FinancesControl.API/Modules/Transactions/Services/TransactionService.cs:71-82` | Create validation checks account, then category. | `_categoryService.ExistsAsync(request.CategoryId)` |
| SERVICE | `server/FinancesControl.API/Modules/Transactions/Services/TransactionService.cs:247-270` | Update validates account, loads transaction, then validates new category. | `_categoryService.ExistsAsync(request.NewCategoryId.Value)` |
| SQL | `server/FinancesControl.API/Modules/TransactionCategories/Repository/SqlCommands/GetTransactionCategoryByIdQuery.cs:12-18` | Existing default-or-owner category visibility predicate. | `AND ("IsDefault" = true OR "UserId" = @UserId);` |
| SQL | `server/FinancesControl.API/Modules/Banks/Repository/SqlCommands/BankExistsQuery.cs:7-18` | Existing user-scoped existence query shape. | `WHERE "Id" = @BankId AND "UserId" = @UserId` |
| TESTS | `server/FinancesControl.Tests/Transactions/Services/TransactionServiceTests.cs:83-90` | Async exception tests use xUnit plus FluentAssertions. | `Assert.ThrowsAsync<ResourceNotFoundException>(...)` |

External research:

- Dapper parameter docs confirm the plan should keep using `DynamicParameters` and parameter placeholders to avoid SQL injection.
- NSubstitute docs confirm argument matchers belong in setup/received contexts; use exact arguments for `TestUserId` where possible.
- xUnit docs confirm the existing `Assert.ThrowsAsync<T>` pattern is appropriate for async service exceptions.

Confidence score: 9/10. The code surface is small, the predicate already exists elsewhere, and the main implementation risk is mechanical call-site/test updates after changing the interface signature.

---

## Amendments

<details>
<summary>2026-07-15T17:29:00-03:00 - Initial plan</summary>

Created implementation plan for scoping transaction category existence checks by default category visibility or current user ownership.

</details>

<details>
<summary>2026-07-15T18:18:55-03:00 - Implementation complete</summary>

Implemented the scoped category existence contract through repository, service, and transaction validation layers. Updated SQL to require matching category ID plus default-or-owner visibility, updated transaction create/update validation to pass the authenticated user ID, and added unit coverage for scoped service delegation plus cross-user category rejection. No product-scope deviations; validation required approved NuGet/network access for targeted test restore.

</details>
