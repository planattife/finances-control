# Implementation Report

**Plan**: `PRPs/plans/add-csrf-strategy-for-cookie-authenticated-requests.plan.md`
**Branch**: `feaute/0065-csfr-strategy-cookies`
**Date**: 2026-07-19
**Status**: COMPLETE

---

## Summary

Implemented a CSRF strategy for cookie-authenticated requests. The backend now issues an authenticated antiforgery request token, validates unsafe authenticated controller actions through a common MVC filter, and the frontend shared Axios client echoes `XSRF-TOKEN` as `X-XSRF-TOKEN` for `POST`, `PUT`, `PATCH`, and `DELETE`.

---

## Assessment vs Reality

| Metric | Predicted | Actual | Reasoning |
| ------ | --------- | ------ | --------- |
| Complexity | MEDIUM | MEDIUM | The cross-cutting surface matched the plan: backend common security, controller attributes, frontend Axios/auth API, and integration tests. |
| Confidence | 8/10 | 8/10 | The central approach was correct; one ASP.NET Core token-cookie detail required a small implementation adjustment. |

Deviation: ASP.NET Core's internal antiforgery cookie is kept separate from the readable SPA request-token cookie. `XSRF-TOKEN` contains the request token for Axios; `.FinancesControl.Antiforgery` remains the HttpOnly server cookie token used by ASP.NET Core validation.

---

## Tasks Completed

| # | Task | File | Status |
| - | ---- | ---- | ------ |
| 1 | Create antiforgery configuration | `server/FinancesControl.API/Common/Antiforgery/ApiAntiforgeryConfiguration.cs` | done |
| 2 | Create validation attribute | `server/FinancesControl.API/Common/Antiforgery/ValidateAntiforgeryTokenAttribute.cs` | done |
| 3 | Wire backend services and token endpoint | `server/FinancesControl.API/Program.cs` | done |
| 4 | Protect logout | `server/FinancesControl.API/Modules/Auth/Controllers/AuthController.cs` | done |
| 5 | Protect finance write actions | finance controllers | done |
| 6 | Configure Axios XSRF behavior | `client/finances-control-web/src/api/axiosClient.ts` | done |
| 7 | Fetch token after login | `client/finances-control-web/src/features/auth/authApi.ts` | done |
| 8 | Add antiforgery tests | `server/FinancesControl.Tests/Common/*Antiforgery*.cs` | done |

---

## Validation Results

| Check | Result | Details |
| ----- | ------ | ------- |
| Frontend lint | passed | `npm.cmd run lint` |
| Backend build | passed | `dotnet build server/FinancesControl.sln` |
| Antiforgery tests | passed | 9 passed, 0 failed |
| Full backend tests | passed | 374 passed, 0 failed |
| Frontend build | passed | Build succeeded; existing Vite chunk-size warning remains |

---

## Files Changed

| File | Action |
| ---- | ------ |
| `server/FinancesControl.API/Common/Antiforgery/ApiAntiforgeryConfiguration.cs` | CREATE |
| `server/FinancesControl.API/Common/Antiforgery/ValidateAntiforgeryTokenAttribute.cs` | CREATE |
| `server/FinancesControl.API/Program.cs` | UPDATE |
| `server/FinancesControl.API/Modules/Auth/Controllers/AuthController.cs` | UPDATE |
| `server/FinancesControl.API/Modules/Accounts/Controllers/AccountController.cs` | UPDATE |
| `server/FinancesControl.API/Modules/Banks/Controllers/BankController.cs` | UPDATE |
| `server/FinancesControl.API/Modules/Cards/Controllers/CardController.cs` | UPDATE |
| `server/FinancesControl.API/Modules/TransactionCategories/Controllers/TransactionCategoryController.cs` | UPDATE |
| `server/FinancesControl.API/Modules/Transactions/Controllers/TransactionController.cs` | UPDATE |
| `client/finances-control-web/src/api/axiosClient.ts` | UPDATE |
| `client/finances-control-web/src/features/auth/authApi.ts` | UPDATE |
| `server/FinancesControl.Tests/Common/ApiAntiforgeryConfigurationTests.cs` | CREATE |
| `server/FinancesControl.Tests/Common/ApiAntiforgeryTests.cs` | CREATE |

---

## Deviations from Plan

- The readable `XSRF-TOKEN` cookie is not the ASP.NET Core antiforgery cookie itself. The implementation writes the returned request token to `XSRF-TOKEN` and keeps the framework cookie as `.FinancesControl.Antiforgery`, which is required for ASP.NET Core's cookie-token plus request-token validation.
- Validation commands used `npm.cmd` instead of `npm` because PowerShell blocks the local `npm.ps1` shim by execution policy.

---

## Issues Encountered

- Initial frontend validation with `npm run build` failed due PowerShell script execution policy. Resolved by running `npm.cmd run build`.
- Initial antiforgery test compile failed due a missing `StatusCodes` import and a FluentAssertions API unavailable in the project's older version. Fixed the test imports/assertion style.

---

## Tests Written

| Test File | Test Cases |
| --------- | ---------- |
| `server/FinancesControl.Tests/Common/ApiAntiforgeryConfigurationTests.cs` | `Configure_WithDefaultOptions_SetsExpectedAntiforgeryOptions`; `CreateRequestTokenCookieOptions_WithDefaultOptions_ReturnsReadableTokenCookieOptions` |
| `server/FinancesControl.Tests/Common/ApiAntiforgeryTests.cs` | anonymous token request rejected; authenticated token request sets readable cookie; finance write without token rejected; logout without token rejected; logout with valid token succeeds; manage info unaffected; login reaches Identity without token |

---

## Next Steps

- [ ] Review implementation
- [ ] Create PR: `gh pr create` or `$prp-pr`
- [ ] Merge when approved
