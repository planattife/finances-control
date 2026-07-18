# Feature: Fix Database Migration Production Safety

## Summary

Update the existing card account migration so production databases with existing `Cards` rows can be upgraded safely. The implementation keeps the project table-script convention, changes the 1.0.1 script to add `"AccountId"` nullable first, backfills it deterministically from existing account-scoped data, adds a foreign key with PostgreSQL's lower-lock `NOT VALID` / `VALIDATE CONSTRAINT` sequence, and only then applies `SET NOT NULL`.

## User Story

As an operator deploying database updates
I want the `Cards.AccountId` migration to handle existing production data safely
So that deployments do not fail or leave cards detached from accounts

## Problem Statement

`server/FinancesControl.API/Database/Tables/1.0.1/v1-0-1_1_add_accountid_column_cards.sql` currently adds `"AccountId" BIGINT NOT NULL` before any default or backfill. On any database where `"Cards"` already has rows, PostgreSQL must reject the migration because existing rows would receive `NULL` for a `NOT NULL` column.

## Solution Statement

Replace the current 1.0.1 migration body with an idempotent production-safe sequence: add `"AccountId" BIGINT` only if missing, backfill all null card account ids using a deterministic mapping from existing `"Transactions"` rows that reference the card and agree on one `"AccountId"`, fail explicitly if any card cannot be backfilled or maps ambiguously, add `"FK_Cards_Accounts_AccountId"` as `NOT VALID` if missing, validate it, and then set `"AccountId"` `NOT NULL`.

## Metadata

| Field            | Value                                                                 |
| ---------------- | --------------------------------------------------------------------- |
| Type             | BUG_FIX                                                               |
| Complexity       | MEDIUM                                                                |
| Systems Affected | PostgreSQL table scripts, Cards table, Transactions data relationship |
| Dependencies     | PostgreSQL SQL server version not documented yet; Dapper 2.1.72; Microsoft.EntityFrameworkCore 8.0.23; Npgsql.EntityFrameworkCore.PostgreSQL 8.0.11 |
| Estimated Tasks  | 4                                                                     |

---

## Lifecycle (append-only)

- **Created:** 2026-07-18T00:00:00-03:00
- **Modified:**
  - 2026-07-18T00:00:00-03:00 - Initial plan created from `PRPs/tasks/fix-database-migration-production-safety.md`.
  - 2026-07-18T18:14:13.0147425-03:00 - Implemented production-safe Cards.AccountId migration and validated backend build/tests.
  - 2026-07-18T18:27:13.7382094-03:00 - Revised migration to avoid same-batch parse errors when clients submit the whole script at once.
- **Commits:** none yet
- **Agent / Session:**
  - Codex GPT-5 - planning session
  - Codex GPT-5 - implementation session
- **Back refs:**
  - `PRPs/tasks/fix-database-migration-production-safety.md` - task brief
  - `docs/database-table-guidelines.md` - SQL table and migration conventions
  - `docs/dapper-usage-guidelines.md` - data access and SQL safety conventions
- **Forward refs:** none yet

> **Append-only:** `Created` is set once; every other field is a list you only ever add to - never overwrite or remove existing entries. Keep references bidirectional: when you add a back/forward ref here, add the reciprocal ref on the other plan.

---

## UX Design

### Before State

```text
+------------------------------------------------------------------------------+
| BEFORE STATE                                                                 |
+------------------------------------------------------------------------------+
| Deployment applies v1-0-1_1_add_accountid_column_cards.sql                    |
|                                                                              |
| Cards table has existing rows -> ADD COLUMN "AccountId" BIGINT NOT NULL       |
|                                                                              |
| Result: PostgreSQL rejects the migration because existing rows have NULL      |
|                                                                              |
| USER_FLOW: Operator deploys database scripts and hits a schema migration error|
| PAIN_POINT: Production upgrade can stop before the API schema matches code    |
| DATA_FLOW: Existing Cards rows have BankId only; AccountId is never derived   |
+------------------------------------------------------------------------------+
```

### After State

```text
+------------------------------------------------------------------------------+
| AFTER STATE                                                                  |
+------------------------------------------------------------------------------+
| Deployment applies v1-0-1_1_add_accountid_column_cards.sql                    |
|                                                                              |
| ADD nullable column -> deterministic backfill from Transactions -> guardrails  |
| -> add FK NOT VALID -> validate FK -> SET NOT NULL                            |
|                                                                              |
| Result: existing valid data is upgraded, unsafe data fails with clear errors  |
|                                                                              |
| USER_FLOW: Operator deploys script and receives either a safe upgrade or an   |
| explicit data-quality error to resolve before retrying                        |
| VALUE_ADD: Cards become account-scoped without breaking production upgrades   |
| DATA_FLOW: Transactions.CardId + Transactions.AccountId populate Cards.AccountId|
+------------------------------------------------------------------------------+
```

### Interaction Changes

| Location | Before | After | User Impact |
|----------|--------|-------|-------------|
| `server/FinancesControl.API/Database/Tables/1.0.1/v1-0-1_1_add_accountid_column_cards.sql` | Adds `AccountId` as `NOT NULL` immediately | Adds nullable, backfills, validates FK, then sets `NOT NULL` | Production migration can succeed on valid existing data |
| Production deployment | Fails generically when `Cards` has existing rows | Fails only on ambiguous or missing backfill data with explicit `RAISE EXCEPTION` messages | Operators know what data must be fixed |
| Runtime card queries | Repository expects `Cards.AccountId` to exist and be non-null | Schema is migrated into the shape expected by code | Existing Cards API behavior remains intact |

---

## Mandatory Reading

**CRITICAL: Implementation agent MUST read these files before starting any task:**

| Priority | File | Lines | Why Read This |
|----------|------|-------|---------------|
| P0 | `PRPs/tasks/fix-database-migration-production-safety.md` | all | Original task scope and expected direction |
| P0 | `docs/database-table-guidelines.md` | all | Project SQL conventions: quoted PascalCase, FK naming, clean scripts, folder naming |
| P0 | `server/FinancesControl.API/Database/Tables/1.0.1/v1-0-1_1_add_accountid_column_cards.sql` | all | The unsafe migration to replace |
| P0 | `server/FinancesControl.API/Database/Tables/1.0.0/v1-0-0_3_create_cards_table.sql` | all | Current initial Cards table shape lacks `AccountId` |
| P0 | `server/FinancesControl.API/Database/Tables/1.0.0/v1-0-0_6_create_transactions_table.sql` | all | Existing deterministic source for card-to-account backfill |
| P1 | `server/FinancesControl.API/Database/Tables/1.0.0/v1-0-0_1_create_accounts_table.sql` | all | Referenced table and key name for FK |
| P1 | `server/FinancesControl.API/Modules/Cards/Repository/SqlCommands/CreateCardCommand.cs` | 10-25 | Runtime insert already writes `Cards.AccountId` |
| P1 | `server/FinancesControl.API/Modules/Cards/Repository/SqlCommands/GetCardByIdQuery.cs` | 9-20 | Runtime reads and filters by `Cards.AccountId` |
| P1 | `server/FinancesControl.API/Modules/Cards/Repository/CardRepository.cs` | 143-177 | Runtime mapper casts `row.AccountId` to non-null `long` |

**External Documentation:**
| Source | Section | Why Needed |
|--------|---------|------------|
| [PostgreSQL ALTER TABLE](https://www.postgresql.org/docs/current/sql-altertable.html) | `ADD COLUMN`, `ADD table_constraint NOT VALID`, `VALIDATE CONSTRAINT`, `SET NOT NULL`, Notes | Confirms nullable add behavior, lock levels, lower-lock FK validation path, and `SET NOT NULL` scan behavior |
| [PostgreSQL Modifying Tables](https://www.postgresql.org/docs/17/ddl-alter.html#DDL-ALTER-ADDING-A-COLUMN) | Adding a Column | Confirms adding a column without a default fills existing rows with null and suggests adding nullable then updating when values are non-default |
| [PostgreSQL UPDATE](https://www.postgresql.org/docs/current/sql-update.html) | Notes on `UPDATE ... FROM` joins | Avoid non-deterministic updates when a join can produce multiple source rows |
| [EF Core 8 Applying Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying#sql-scripts) | SQL scripts and production deployment | Reinforces reviewable SQL scripts for production and warns against direct runtime migration in production |
| [EF Core 8 Breaking Changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/breaking-changes) | Summary / Target Framework | Confirms EF Core 8 targets .NET 8; listed high-impact changes do not alter this raw SQL table-script plan |
| [Npgsql EF Core Provider 8.0 Release Notes](https://www.npgsql.org/efcore/release-notes/8.0.html#breaking-changes) | Breaking changes | Provider assumes PostgreSQL 14 by default; exact server version is not documented yet, but this plan uses long-standing PostgreSQL SQL forms |
| [Npgsql 8.0 Release Notes](https://www.npgsql.org/doc/release-notes/8.0.html#breaking-changes) | Dynamic feature opt-ins | No JSON/unmapped-type/tuple behavior is involved, so these breaking changes are not implementation blockers |
| [Dapper 2.1.72 NuGet](https://www.nuget.org/packages/Dapper/2.1.72) | README / package metadata | Confirms version and Dapper as a raw SQL micro-ORM; no migration-script behavior is provided by Dapper |
| [Dapper 2.1.72 Release Notes](https://github.com/DapperLib/Dapper/releases/tag/2.1.72) | What's Changed | 2.1.72 changes are packaging/bug fixes; no relevant SQL migration behavior change |

---

## Patterns to Mirror

**NAMING_CONVENTION:**

```sql
-- SOURCE: server/FinancesControl.API/Database/Tables/1.0.0/v1-0-0_6_create_transactions_table.sql:14-17
CONSTRAINT "FK_Transactions_TransactionCategories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "TransactionCategories"("Id") ON DELETE RESTRICT,
CONSTRAINT "FK_Transactions_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts"("Id") ON DELETE RESTRICT,
CONSTRAINT "FK_Transactions_Banks_BankId" FOREIGN KEY ("BankId") REFERENCES "Banks"("Id") ON DELETE RESTRICT,
CONSTRAINT "FK_Transactions_Cards_CardId" FOREIGN KEY ("CardId") REFERENCES "Cards"("Id") ON DELETE RESTRICT
```

**ERROR_HANDLING:**

```sql
-- SOURCE: server/FinancesControl.API/Database/Tables/1.0.1/v1-0-1_1_add_accountid_column_cards.sql:1-15
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'Cards' 
        AND column_name = 'AccountId'
    ) THEN
        ALTER TABLE "Cards" 
        ADD COLUMN "AccountId" BIGINT NOT NULL;
        
        ALTER TABLE "Cards" 
        ADD CONSTRAINT "FK_Cards_Accounts_AccountId" 
        FOREIGN KEY ("AccountId") REFERENCES "Accounts"("Id") ON DELETE RESTRICT;
    END IF;
END $$;
```

**LOGGING_PATTERN:**

```text
No database migration logging convention is documented yet. Use PostgreSQL RAISE EXCEPTION only for blocking data-safety guardrails; do not add application logging.
```

**REPOSITORY_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Modules/Cards/Repository/SqlCommands/GetCardByIdQuery.cs:9-20
var sql = @"
    SELECT c.""Id"", c.""CardType"", c.""CardNumber"", c.""ExpirationDate"", c.""IsActive"", 
           c.""CardNetwork"", c.""BankId"", b.""Name"" AS ""BankName"", c.""AccountId"", 
           c.""CardName"", c.""CreatedAt"", c.""UpdatedAt""
    FROM ""Cards"" c
    INNER JOIN ""Banks"" b ON c.""BankId"" = b.""Id""
    WHERE c.""Id"" = @Id AND c.""AccountId"" = @AccountId;
";

var parameters = new DynamicParameters();
parameters.Add("@Id", cardId);
parameters.Add("@AccountId", accountId);
```

**SERVICE_PATTERN:**

```csharp
// SOURCE: server/FinancesControl.API/Modules/Cards/Repository/CardRepository.cs:143-157
return Card.FromDatabase(
    id: (long)row.Id,
    cardNumber: row.CardNumber,
    expirationDate: row.ExpirationDate,
    cardNetwork: (CardNetwork)row.CardNetwork,
    bankId: (long)row.BankId,
    accountId: (long)row.AccountId,
    cardType: (CardType)row.CardType,
    isActive: row.IsActive,
    cardName: row.CardName,
    createdAt: row.CreatedAt,
    updatedAt: row.UpdatedAt,
    bankName: row.BankName
);
```

**TEST_STRUCTURE:**

```text
No SQL migration test harness is documented yet. Existing backend validation uses:
dotnet build server/FinancesControl.sln
dotnet test server/FinancesControl.sln
```

---

## Files to Change

| File | Action | Justification |
| ---- | ------ | ------------- |
| `server/FinancesControl.API/Database/Tables/1.0.1/v1-0-1_1_add_accountid_column_cards.sql` | UPDATE | Make the production migration safe and idempotent |

---

## NOT Building (Scope Limits)

Explicit exclusions to prevent scope creep:

- Do not create EF Core migration classes; this repository uses hand-authored SQL scripts under `Database/Tables`.
- Do not change API, repository, DTO, or frontend behavior; runtime code already expects account-scoped cards.
- Do not modify the already released 1.0.0 Cards creation script unless the implementer confirms this project treats 1.0.0 scripts as mutable before production use. The task specifically names the 1.0.1 migration.
- Do not infer `Cards.AccountId` from `Banks` alone because `Banks` currently has `UserId` but no `AccountId` in the table script.

---

## Step-by-Step Tasks

Execute in order. Each task is atomic and independently verifiable.

**Status markers** - prefix EVERY task header with one; the build agent updates it inline as it works: `[ ]` idle . `[wip]` in progress . `[x]` complete . `[f]` failed. All tasks start `[ ]`. If a task cannot be made to pass, mark it `[f]`, record why in Agent Notes, and move on if the rest of the plan can still proceed.

### `[x]` Task 1: UPDATE `server/FinancesControl.API/Database/Tables/1.0.1/v1-0-1_1_add_accountid_column_cards.sql`

- **ACTION**: Replace the unsafe immediate `NOT NULL` add with nullable column creation.
- **IMPLEMENT**: Use `ALTER TABLE "Cards" ADD COLUMN IF NOT EXISTS "AccountId" BIGINT;`. Keep quoted PascalCase identifiers. Do not add a default.
- **MIRROR**: `server/FinancesControl.API/Database/Tables/1.0.0/v1-0-0_6_create_transactions_table.sql:8` - use `"AccountId" BIGINT` for account foreign keys.
- **PATTERN**: PostgreSQL `ADD COLUMN IF NOT EXISTS` gives idempotence without information-schema table-name ambiguity.
- **GOTCHA**: PostgreSQL docs confirm `ADD COLUMN` without a default fills existing rows with null, which is safe only because later tasks backfill before `SET NOT NULL`.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 2: UPDATE `server/FinancesControl.API/Database/Tables/1.0.1/v1-0-1_1_add_accountid_column_cards.sql`

- **ACTION**: Add deterministic backfill and blocking guardrails.
- **IMPLEMENT**: Backfill only cards where all existing transactions referencing that card agree on exactly one account:
  - Build a grouped source from `"Transactions"` with `"CardId" IS NOT NULL`, grouped by `"CardId"`, selecting `MIN("AccountId")` as the only account when `COUNT(DISTINCT "AccountId") = 1`.
  - `UPDATE "Cards" c SET "AccountId" = source."AccountId" FROM source WHERE c."Id" = source."CardId" AND c."AccountId" IS NULL;`
  - Add a `DO $$` guard that raises if any card with transactions has multiple distinct transaction account ids.
  - Add a `DO $$` guard that raises if any `"Cards"."AccountId"` remains null after backfill.
- **MIRROR**: `server/FinancesControl.API/Database/Tables/1.0.0/v1-0-0_6_create_transactions_table.sql:10-17` - transactions already model the relationship between Account, Bank, and Card.
- **PATTERN**: Aggregate before `UPDATE ... FROM` so each card has at most one source row.
- **GOTCHA**: PostgreSQL warns that `UPDATE ... FROM` can be unpredictable if the join produces more than one row for a target row; grouping and `HAVING COUNT(DISTINCT ...) = 1` mitigates this.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 3: UPDATE `server/FinancesControl.API/Database/Tables/1.0.1/v1-0-1_1_add_accountid_column_cards.sql`

- **ACTION**: Add and validate the account foreign key in a production-friendly sequence.
- **IMPLEMENT**: Add `"FK_Cards_Accounts_AccountId"` only if it does not already exist using `pg_constraint`, with:
  `ALTER TABLE "Cards" ADD CONSTRAINT "FK_Cards_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts"("Id") ON DELETE RESTRICT NOT VALID;`
  Then always run:
  `ALTER TABLE "Cards" VALIDATE CONSTRAINT "FK_Cards_Accounts_AccountId";`
- **MIRROR**: `server/FinancesControl.API/Database/Tables/1.0.0/v1-0-0_3_create_cards_table.sql:1` - use `ON DELETE RESTRICT` and the `FK_Cards_...` naming shape.
- **PATTERN**: Use `pg_constraint` by constraint name and table regclass to avoid relying on information-schema casing behavior.
- **GOTCHA**: PostgreSQL `NOT VALID` avoids the initial full-table validation scan, but new rows are still checked; `VALIDATE CONSTRAINT` must be run before `SET NOT NULL` acceptance criteria.
- **VALIDATE**: `dotnet build server/FinancesControl.sln`

### `[x]` Task 4: UPDATE `server/FinancesControl.API/Database/Tables/1.0.1/v1-0-1_1_add_accountid_column_cards.sql`

- **ACTION**: Set the column to `NOT NULL` after successful backfill and FK validation.
- **IMPLEMENT**: End the script with `ALTER TABLE "Cards" ALTER COLUMN "AccountId" SET NOT NULL;`. Keep no comments and avoid empty lines if following `docs/database-table-guidelines.md` strictly.
- **MIRROR**: `server/FinancesControl.API/Modules/Cards/Repository/CardRepository.cs:151` - runtime casts `row.AccountId` to non-null `long`, so the schema must guarantee non-null data.
- **PATTERN**: Constraint enforcement is ordered as nullable add -> data repair -> referential integrity -> non-nullability.
- **GOTCHA**: PostgreSQL `SET NOT NULL` scans the table unless a valid check/not-null proof exists. This is acceptable for correctness, but schedule the migration with that lock/scan in mind.
- **VALIDATE**: `dotnet test server/FinancesControl.sln`

---

## Testing Strategy

### Unit Tests to Write

| Test File | Test Cases | Validates |
| --------- | ---------- | --------- |
| Not documented yet | No SQL migration test harness exists in the repository | Use build, full test suite, and manual database smoke scripts |

### Edge Cases Checklist

- [ ] Empty `"Cards"` table: script completes, FK exists, column is `NOT NULL`.
- [ ] Existing card with transactions in exactly one account: card receives that account id.
- [ ] Existing card with no transactions: script raises a clear exception instead of choosing an arbitrary account.
- [ ] Existing card with transactions in multiple accounts: script raises a clear ambiguity exception.
- [ ] FK already exists from a previous partial run: script does not fail on duplicate constraint creation.
- [ ] Column already exists but contains nulls: script backfills null values before setting `NOT NULL`.
- [ ] Column already exists and is populated: script validates constraint and enforces `NOT NULL`.

---

## Validation Commands

**Validation loop:** the plan is not complete until every command below passes (exit 0). On any failure, fix the cause and re-run - loop until all pass. If a check is genuinely impossible, mark it `[f]`, note why in Agent Notes, and move on.

### Level 1: STATIC_ANALYSIS

```bash
dotnet build server/FinancesControl.sln
```

**EXPECT**: Exit 0, no compile errors

### Level 2: UNIT_TESTS

```bash
dotnet test server/FinancesControl.sln
```

**EXPECT**: All backend tests pass

### Level 3: FULL_SUITE

```bash
dotnet test server/FinancesControl.sln
```

**EXPECT**: All backend tests pass; no frontend change is in scope

### Level 4: DATABASE_VALIDATION (if schema changes)

Run against a disposable PostgreSQL database loaded with production-like fixtures:

- [ ] Apply 1.0.0 table scripts in order.
- [ ] Insert one account, one bank, at least one card, and one transaction referencing that card/account.
- [ ] Apply `server/FinancesControl.API/Database/Tables/1.0.1/v1-0-1_1_add_accountid_column_cards.sql`.
- [ ] Verify `"Cards"."AccountId"` is populated and `is_nullable = 'NO'`.
- [ ] Verify `pg_constraint` contains a validated `"FK_Cards_Accounts_AccountId"`.
- [ ] Repeat with a card that has no transaction and confirm the script fails with the planned explicit exception.
- [ ] Repeat with one card used by transactions in two accounts and confirm the script fails with the planned explicit exception.

### Level 5: BROWSER_VALIDATION (if UI changes)

Not applicable; no UI changes are planned.

### Level 6: MANUAL_VALIDATION

1. Review the final SQL script for quoted PascalCase names and `ON DELETE RESTRICT`.
2. Confirm the script contains no data-destructive statements.
3. Confirm there is no fallback that chooses `MIN(AccountId)` across multiple accounts without first rejecting ambiguity.
4. Confirm rerunning the script is safe after a successful run.
5. Confirm deployment notes mention that cards with no transaction history require a manual account mapping before this migration can complete.

---

## Acceptance Criteria

- [ ] `v1-0-1_1_add_accountid_column_cards.sql` no longer adds `"AccountId" BIGINT NOT NULL` before backfill.
- [ ] The script adds `"AccountId"` nullable when the column is missing.
- [ ] The script deterministically backfills from `"Transactions"` only when each card maps to exactly one account.
- [ ] The script fails explicitly for cards with no derivable account.
- [ ] The script fails explicitly for cards with ambiguous transaction account mappings.
- [ ] The script adds `"FK_Cards_Accounts_AccountId"` with `ON DELETE RESTRICT`.
- [ ] The script validates the FK before setting `"AccountId"` `NOT NULL`.
- [ ] `dotnet build server/FinancesControl.sln` passes.
- [ ] `dotnet test server/FinancesControl.sln` passes.
- [ ] Manual disposable PostgreSQL validation covers success, missing backfill, and ambiguous backfill cases.

---

## Completion Checklist

- [ ] All tasks completed in dependency order
- [ ] Each task validated immediately after completion
- [ ] Level 1: Static analysis (build) passes
- [ ] Level 2: Unit tests pass
- [ ] Level 3: Full backend suite succeeds
- [ ] Level 4: Database validation passes
- [ ] Level 5: Browser validation marked not applicable
- [ ] All acceptance criteria met

---

## Risks and Mitigations

| Risk | Likelihood | Impact | Mitigation |
| ---- | ---------- | ------ | ---------- |
| Existing cards have no transactions, so `AccountId` cannot be derived | MED | HIGH | Fail with an explicit exception; require manual mapping before retrying |
| A card appears in transactions under multiple accounts | LOW | HIGH | Detect with `COUNT(DISTINCT "AccountId") > 1` and fail before updating |
| `UPDATE ... FROM` produces non-deterministic results | MED | HIGH | Aggregate to one source row per card before the update |
| `SET NOT NULL` locks/scans the Cards table | MED | MED | Perform after backfill; schedule migration during a safe deployment window |
| PostgreSQL server version is not documented yet | MED | MED | Use long-standing PostgreSQL features and validate against the actual deployed version before production |
| Existing script clean-script guideline says no comments/empty lines, while guardrails benefit readability | MED | LOW | Favor docs: final SQL should be comment-free and can use compact formatting, while keeping guardrail names/messages clear |

---

## Questionables

<details>
<summary>How should cards with no transaction history be assigned?</summary>

The plan assumes there is no safe deterministic assignment from current schema because `"Banks"` has `"UserId"` but no `"AccountId"` in `v1-0-0_2_create_banks_table.sql`. Therefore the migration should fail explicitly for those rows instead of guessing.

</details>

<details>
<summary>Should the 1.0.0 create Cards table also be updated to include AccountId?</summary>

The task names both the 1.0.0 Cards-related scripts and the 1.0.1 migration, but the production-safety issue is in the additive migration. This plan keeps the file change scoped to the 1.0.1 migration. If this project expects baseline scripts to represent a freshly installed final schema, update `v1-0-0_3_create_cards_table.sql` in a separate explicit task.

</details>

---

## Agent Notes

Phase 2 codebase findings:

| Category | File:Lines | Pattern Description | Code Snippet |
| -------- | ---------- | ------------------- | ------------ |
| DATABASE | `server/FinancesControl.API/Database/Tables/1.0.0/v1-0-0_3_create_cards_table.sql:1` | Initial Cards table has `BankId` FK but no `AccountId` | `"BankId" BIGINT NOT NULL ... CONSTRAINT "FK_Cards_Banks_BankId"` |
| DATABASE | `server/FinancesControl.API/Database/Tables/1.0.1/v1-0-1_1_add_accountid_column_cards.sql:8-13` | Current migration adds non-null column before backfill | `ADD COLUMN "AccountId" BIGINT NOT NULL` |
| DATABASE | `server/FinancesControl.API/Database/Tables/1.0.0/v1-0-0_6_create_transactions_table.sql:8-17` | Transactions already link AccountId and CardId | `"AccountId" BIGINT NOT NULL`, `"CardId" BIGINT NULL` |
| RUNTIME | `server/FinancesControl.API/Modules/Cards/Repository/SqlCommands/CreateCardCommand.cs:11-24` | New card creation already writes AccountId | `INSERT INTO "Cards" (... "AccountId" ...)` |
| RUNTIME | `server/FinancesControl.API/Modules/Cards/Repository/SqlCommands/GetAllCardsQuery.cs:11-17` | Card reads select and filter by AccountId | `WHERE c."AccountId" = @AccountId` |
| RUNTIME | `server/FinancesControl.API/Modules/Cards/Repository/CardRepository.cs:143-177` | Card mapper requires non-null row.AccountId | `accountId: (long)row.AccountId` |

External research findings:

- [PostgreSQL ALTER TABLE](https://www.postgresql.org/docs/current/sql-altertable.html)
  - KEY_INSIGHT: `ADD COLUMN` without a default does not rewrite the table and existing rows receive null; `SET NOT NULL` is allowed only when no nulls exist.
  - APPLIES_TO: nullable-add and final non-null tasks in `v1-0-1_1_add_accountid_column_cards.sql`.
  - GOTCHA: `SET NOT NULL` normally scans the table; mitigate by running after backfill and planning a deployment window.
- [PostgreSQL ALTER TABLE - NOT VALID / VALIDATE CONSTRAINT](https://www.postgresql.org/docs/current/sql-altertable.html)
  - KEY_INSIGHT: `ADD CONSTRAINT ... NOT VALID` skips the initial scan while still enforcing new rows; `VALIDATE CONSTRAINT` later scans existing rows with less intrusive locking than the initial add.
  - APPLIES_TO: FK addition on `"Cards"("AccountId")`.
  - GOTCHA: `NOT VALID` alone is not enough; acceptance requires `VALIDATE CONSTRAINT`.
- [PostgreSQL UPDATE](https://www.postgresql.org/docs/current/sql-update.html)
  - KEY_INSIGHT: `UPDATE ... FROM` should not join one target row to multiple source rows.
  - APPLIES_TO: card account backfill.
  - GOTCHA: Aggregate transaction data by card and reject ambiguous cards before or during backfill.
- [EF Core Applying Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying#sql-scripts)
  - KEY_INSIGHT: Microsoft recommends inspectable SQL scripts for production deployment.
  - APPLIES_TO: keeping this as a hand-authored SQL-script change rather than runtime `MigrateAsync()`.
  - GOTCHA: EF Core docs warn runtime production migrations can require elevated permissions and have concurrency hazards in EF versions before 9; this project uses EF Core 8.0.23.
- [Npgsql EF Core 8.0 Release Notes](https://www.npgsql.org/efcore/release-notes/8.0.html#breaking-changes)
  - KEY_INSIGHT: Provider default PostgreSQL compatibility mode was bumped to 14.
  - APPLIES_TO: deployment validation against actual PostgreSQL version.
  - GOTCHA: The project has no documented PostgreSQL server version; verify the target version before production.
- [Dapper 2.1.72 Release Notes](https://github.com/DapperLib/Dapper/releases/tag/2.1.72)
  - KEY_INSIGHT: 2.1.72 has no relevant migration-script behavior; Dapper remains runtime SQL execution only.
  - APPLIES_TO: no implementation changes needed in repositories.
  - GOTCHA: Do not use Dapper docs to justify migration execution behavior; PostgreSQL docs govern the SQL script.

Conflict check between external docs and codebase patterns:

- PostgreSQL docs recommend `NOT VALID` / `VALIDATE CONSTRAINT` for lower-lock FK rollout. Existing database scripts add FKs inline during table creation or immediate alter; no existing production-safe additive FK pattern was found. This is an intentional improvement for the additive migration, not a conflict with runtime code.
- EF Core docs recommend production SQL scripts; this matches the repository's `Database/Tables` script convention.
- Npgsql docs recommend awareness of PostgreSQL compatibility version. Project PostgreSQL server version is not documented yet; plan includes validation instead of assuming.
- Dapper parameter guidance aligns with runtime repository patterns, but this task is static SQL and does not use application input.

Architecture decision:

APPROACH_CHOSEN: Preserve the existing `Cards.AccountId` runtime contract and repair only the database migration path. Use `Transactions` as the deterministic existing source because it is the only table in the discovered schema that links cards to accounts.

RATIONALE: Card repository SQL already inserts, selects, filters, and maps `"AccountId"`. Changing runtime behavior would widen scope and risk regressions. The unsafe part is the schema transition for existing data.

ALTERNATIVES_REJECTED:

- Backfill from `"Banks"`: rejected because `Banks` has `"UserId"` but no `"AccountId"`, so a user with multiple accounts cannot be mapped deterministically.
- Add a default account id: rejected because it would corrupt account isolation.
- Leave FK unvalidated: rejected because runtime assumes every card belongs to a valid account.
- Add the column as nullable permanently: rejected because runtime maps `row.AccountId` to non-null `long` and account-scoped card access depends on it.

---

## Amendments

<details>
<summary>2026-07-18T00:00:00-03:00 - Initial plan</summary>

Created implementation plan for production-safe `Cards.AccountId` migration based on codebase exploration and official PostgreSQL, EF Core, Npgsql, and Dapper documentation.

</details>

<details>
<summary>2026-07-18T18:14:13.0147425-03:00 - Implemented migration safety fix</summary>

Replaced the unsafe immediate `BIGINT NOT NULL` add with a nullable add, ambiguity guard, deterministic grouped backfill from `Transactions`, null guard, `NOT VALID` foreign-key add, constraint validation, and final `SET NOT NULL`. `dotnet build server/FinancesControl.sln` and `dotnet test server/FinancesControl.sln` passed. Disposable PostgreSQL smoke validation was not run because no local database harness is documented yet.

</details>

<details>
<summary>2026-07-18T18:27:13.7382094-03:00 - Fixed same-batch PostgreSQL parse issue</summary>

Manual database execution showed PostgreSQL could report `column c.AccountId does not exist` when the whole file was submitted as one script and later top-level statements referenced the newly added column. Revised the migration into a single `DO` block that adds the column first and executes dependent SQL dynamically after the column exists. `dotnet build server/FinancesControl.sln` and `dotnet test server/FinancesControl.sln` passed. A local `psql` smoke test was not run because `psql` is not installed in this environment.

</details>
