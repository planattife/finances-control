# Backend Testing Guidelines

## Scope

These guidelines document the current backend test conventions used by `server/FinancesControl.Tests`.

All new backend features and code changes must include tests that cover the new behavior. When a change intentionally does not require a test, the reason must be explicit in the implementation notes or pull request description.

## Test Project

- Test project: `server/FinancesControl.Tests`
- Target framework: `.NET 9.0` (`net9.0`)
- API project reference: `server/FinancesControl.API`
- Test command: `dotnet test server/FinancesControl.sln`

## Frameworks And Versions

The test project currently uses these packages from `FinancesControl.Tests.csproj`:

- `xunit` `2.9.2`
- `xunit.runner.visualstudio` `2.8.2`
- `Microsoft.NET.Test.Sdk` `17.12.0`
- `NSubstitute` `5.3.0`
- `FluentAssertions` `4.19.4`
- `coverlet.collector` `6.0.2`

## Folder And Naming Pattern

Tests are organized by backend module, mirroring the API module structure:

```text
server/FinancesControl.Tests/
  Accounts/
    Controllers/
    Models/
    Services/
  Banks/
    Controllers/
    Models/
    Services/
  Cards/
    Controllers/
    Models/
    Services/
  TransactionCategories/
    Controllers/
    Models/
    Services/
  Transactions/
    Controllers/
    Models/
    Services/
  Common/
```

Use the same structure for new modules. Test class names should follow the production class name plus `Tests`, such as `AccountServiceTests`, `CardControllerTests`, or `TransactionTests`.

Test method names follow the current behavior-focused pattern:

```text
MethodOrScenario_WithCondition_ExpectedResult
```

Examples:

- `CreateAccount_WithValidRequest_ReturnsNewAccountId`
- `GetById_WithInvalidId_ReturnsNull`
- `Constructor_WithInvalidName_ThrowsArgumentException`
- `InvokeAsync_WithUnexpectedException_ReturnsGenericErrorWithoutDetails`

## Files That Require Tests

Add or update tests whenever changing backend behavior in these areas:

- Domain models and entities: test constructors, validation rules, state transitions, timestamps, and domain exceptions.
- Services: test business rules, repository/service collaboration, user scoping, pagination metadata, response mapping, and exception paths.
- Controllers: test HTTP result types, status codes, route values, response bodies, authenticated user propagation, and service calls.
- Common middleware and shared infrastructure: test status-code mapping, response shape, sensitive-detail hiding, and shared error behavior.
- DTO or request validation behavior when validation rules affect API behavior.

Repository and SQL behavior is not covered by a documented test pattern yet. If repository logic changes, prefer adding focused tests when a practical pattern exists nearby; otherwise call out that repository testing is not documented yet.

## Test Style

- Use xUnit `[Fact]` for single-case tests.
- Use xUnit `[Theory]` with `[InlineData]` for equivalent input variations.
- Use `NSubstitute` for collaborators such as repositories and dependent services.
- Use `FluentAssertions` for expressive assertions where practical.
- Existing tests also use direct xUnit assertions such as `Assert.Throws` and `Assert.ThrowsAsync`, especially for exception paths.
- Arrange dependencies in the test constructor when they are reused across tests.
- Keep test data local to each test unless helper methods make repeated setup clearer.
- Use private helper methods for repeated setup, such as creating response objects or configuring common substitutes.
- Prefer testing observable behavior over implementation details, while still verifying important collaborator calls with `Received`.

## Coverage Expectations For New Work

Every new feature or code change must include tests for:

- The successful path.
- Validation failures and domain/business-rule failures.
- Not-found or unauthorized user-scope behavior when applicable.
- Important interactions with repositories or dependent services.
- Pagination, filtering, sorting, or response metadata when those behaviors are introduced or changed.
- Exception handling or middleware behavior when shared error handling changes.

Bug fixes must include a regression test that fails before the fix and passes after the fix whenever practical.

## Validation

Run the backend test suite before completing backend work:

```bash
dotnet test server/FinancesControl.sln
```

If tests cannot be run, document the reason clearly.
