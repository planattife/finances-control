# Implementation Report

**Plan**: `PRPs/plans/fix-database-migration-production-safety.plan.md`
**Branch**: `feature/0060-database-migration-scripts-fix`
**Date**: 2026-07-18
**Status**: COMPLETE

---

## Summary

Implemented the production-safe `Cards.AccountId` migration sequence in the existing 1.0.1 table script. The migration now adds the column nullable first, rejects ambiguous card/account mappings, backfills only deterministic mappings from `Transactions`, rejects remaining null mappings, validates the account FK, and only then sets the column `NOT NULL`.

Follow-up manual execution revealed that submitting the whole file as one script could make PostgreSQL analyze top-level statements before the new column was visible. The script was revised to use a single `DO` block and dynamic `EXECUTE` for SQL that depends on the newly added column.

---

## Assessment vs Reality

| Metric | Predicted | Actual | Reasoning |
| ------ | --------- | ------ | --------- |
| Complexity | MEDIUM | MEDIUM | The implementation touched one SQL script, but preserving account isolation required explicit guardrails. |
| Confidence | 8/10 | 8/10 | The migration order was correct, but manual validation exposed a same-batch parse issue that required dynamic SQL. |

Implementation follow-up: dependent SQL now executes dynamically inside the migration block so it is parsed after `"Cards"."AccountId"` exists.

---

## Tasks Completed

| # | Task | File | Status |
| - | ---- | ---- | ------ |
| 1 | Add `Cards.AccountId` as nullable | `server/FinancesControl.API/Database/Tables/1.0.1/v1-0-1_1_add_accountid_column_cards.sql` | Complete |
| 2 | Add deterministic backfill and guardrails | `server/FinancesControl.API/Database/Tables/1.0.1/v1-0-1_1_add_accountid_column_cards.sql` | Complete |
| 3 | Add and validate account FK | `server/FinancesControl.API/Database/Tables/1.0.1/v1-0-1_1_add_accountid_column_cards.sql` | Complete |
| 4 | Set `Cards.AccountId` `NOT NULL` after validation | `server/FinancesControl.API/Database/Tables/1.0.1/v1-0-1_1_add_accountid_column_cards.sql` | Complete |

---

## Validation Results

| Check | Result | Details |
| ----- | ------ | ------- |
| Build | Passed | `dotnet build server/FinancesControl.sln` exited 0 after the dynamic SQL revision. |
| Unit tests | Passed | `dotnet test server/FinancesControl.sln`: 355 passed, 0 failed, 0 skipped after the dynamic SQL revision. |
| Full suite | Passed | Backend test command is the full suite for this scoped SQL change. |
| Database smoke | Partial manual | User manually found the same-batch parse issue; local `psql` is unavailable here, so final database rerun remains manual. |
| Browser | N/A | No UI changes. |

---

## Files Changed

| File | Action | Lines |
| ---- | ------ | ----- |
| `server/FinancesControl.API/Database/Tables/1.0.1/v1-0-1_1_add_accountid_column_cards.sql` | UPDATE | Replaced unsafe migration body |
| `PRPs/plans/completed/fix-database-migration-production-safety.plan.md` | UPDATE | Task markers and lifecycle/amendment notes |
| `PRPs/reports/fix-database-migration-production-safety-report.md` | CREATE | Implementation report |

---

## Deviations from Plan

The original implementation used top-level SQL statements after the column add. Manual execution showed PostgreSQL could raise `column c.AccountId does not exist` when the whole script was submitted as one batch. The script now uses dynamic SQL for statements depending on the new column.

---

## Issues Encountered

Manual database execution exposed a same-batch parse/analysis issue. Resolved by moving the migration into one `DO` block and executing dependent SQL dynamically after the column add.

---

## Tests Written

No tests were added because the repository has no documented SQL migration test harness. Existing backend build and test validation passed.

---

## Next Steps

- [ ] Review implementation.
- [ ] Run disposable PostgreSQL validation for success, missing mapping, and ambiguous mapping cases before production deployment.
- [ ] Create PR when ready.
