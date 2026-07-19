# Development Register Endpoint Access

## Problem Statement

The only developer on the project needs to create test users while working locally. After the default ASP.NET Core Identity endpoints were restricted, `POST /register` is no longer reachable in development, which blocks manual frontend testing of authenticated pages and makes it harder to inspect registration API responses during implementation.

## Evidence

- User report: manual testing is blocked because there is no convenient way to register a user and access authenticated frontend pages during development.
- Existing technical context: `PRPs/plans/completed/restrict-public-authentication-endpoints.plan.md` intentionally restricted the default Identity endpoint surface to avoid unsupported public auth flows.
- Current code observation: `server/FinancesControl.API/Common/IdentityEndpointRouteBuilderExtensions.cs` allows only `POST /login` and `GET /manage/info`; every other Identity endpoint receives restricted authorization metadata.

## Proposed Solution

Allow `POST /register` only when the API is running in the Development environment. The endpoint should remain public without authentication in development so the developer can create test users through Swagger, API clients, or frontend flows, while production, testing, and any non-development environment keep the current restricted behavior for register and all other hidden Identity endpoints.

## Key Hypothesis

We believe development-only public access to `POST /register` will unblock authenticated frontend testing for the project developer.
We'll know we're right when `POST /register` appears and works without auth in Development, while the same endpoint remains restricted outside Development.

## What We're NOT Building

- A production public registration feature - the product has not decided to support open signup.
- Invite-based registration or admin-managed user creation - this request only addresses local development testing.
- Password reset, email confirmation, refresh token, resend confirmation, or 2FA endpoint exposure - those remain unsupported hidden Identity endpoints.
- A frontend registration page redesign - direct API access is enough for this development workflow.

## Success Metrics

| Metric | Target | How Measured |
| ------ | ------ | ------------ |
| Development register access | `POST /register` accepts anonymous requests in Development | Integration test or manual API call with Development environment |
| Non-development safety | `POST /register` remains restricted outside Development | Integration test with a non-Development environment |
| Endpoint scope control | No other previously hidden Identity endpoint becomes public | Existing and/or updated Identity endpoint authorization tests |

## Open Questions

- [ ] Should the Testing environment keep `POST /register` restricted, or should specific integration tests be allowed to opt into development-style behavior?
- [ ] Should Swagger visibility be explicitly tested, or is runtime endpoint behavior sufficient for this phase?

---

## Users & Context

**Primary User**

- **Who**: The sole developer of the Finances Control project.
- **Current behavior**: Attempts to test authenticated frontend pages but cannot easily create a valid user after Identity register was restricted.
- **Trigger**: Starting frontend fixes that require logging into authenticated areas of the app.
- **Success state**: A test user can be registered in Development and then used to sign into the frontend.

**Job to Be Done**
When I am developing authenticated frontend flows, I want to register a dev test user without authentication, so I can log in and validate the UI and API responses end to end.

**Non-Users**
Production users and external visitors are not the target. This is not a public signup capability.

---

## Solution Detail

### Core Capabilities (MoSCoW)

| Priority | Capability | Rationale |
| -------- | ---------- | --------- |
| Must | Allow anonymous `POST /register` in Development | Unblocks local test-user creation |
| Must | Keep `POST /register` restricted outside Development | Preserves the previous production security decision |
| Must | Keep other hidden Identity endpoints restricted | Avoids reopening unsupported auth flows |
| Should | Cover behavior with backend tests | Prevents accidental environment leakage later |
| Could | Verify Swagger visibility for Development | Helpful for manual testing, but runtime access is the key behavior |
| Won't | Build a frontend registration experience | Not needed for the immediate development workflow |

### MVP Scope

Update the restricted Identity endpoint mapping so `POST /register` is considered allowed only when `app.Environment.IsDevelopment()` is true. Add targeted tests for development and non-development behavior.

### User Flow

1. Developer runs the API in Development.
2. Developer calls `POST /register` from Swagger, an API client, or a frontend test flow.
3. API creates the Identity user without requiring an existing authenticated session.
4. Developer logs in and reaches authenticated frontend pages for manual validation.

---

## Technical Approach

**Feasibility**: HIGH

**Architecture Notes**

- The implementation should likely extend `MapRestrictedIdentityApi<TUser>()` in `server/FinancesControl.API/Common/IdentityEndpointRouteBuilderExtensions.cs`.
- `Program.cs` already has access to `app.Environment`, so the mapping helper can accept environment context or a development-only flag.
- The existing allow-list method can be expanded from static route/method checks to route/method plus environment-aware checks.
- No Dapper, database table, or frontend API structure changes are expected.

**Technical Risks**

| Risk | Likelihood | Mitigation |
| ---- | ---------- | ---------- |
| Register accidentally becomes public outside Development | MED | Add non-development tests around `POST /register` |
| Existing restricted endpoint tests assume register is always blocked | HIGH | Update tests to split Development vs non-Development expectations |
| Swagger visibility differs from runtime authorization | MED | Treat runtime behavior as authoritative; add Swagger assertion only if simple |

---

## Implementation Phases

| # | Phase | Description | Status | Parallel | Depends | PRP Plan |
| - | ----- | ----------- | ------ | -------- | ------- | -------- |
| 1 | Environment-Aware Mapping | Allow `POST /register` only for Development inside the Identity endpoint helper | complete | - | - | `PRPs/plans/completed/development-register-endpoint-access.plan.md` |
| 2 | Backend Tests | Cover Development access and non-Development restriction for register | pending | - | 1 | - |
| 3 | Validation | Run targeted Identity tests and full backend test suite | pending | - | 2 | - |

### Phase Details

**Phase 1: Environment-Aware Mapping**

- **Goal**: Preserve the existing Identity endpoint restriction while adding a Development-only exception for registration.
- **Scope**: Backend startup/helper only.
- **Success signal**: `POST /register` is classified as allowed only for Development.

**Phase 2: Backend Tests**

- **Goal**: Prove the environment boundary.
- **Scope**: Update or add Identity endpoint tests.
- **Success signal**: Tests fail if register becomes public in non-development environments or remains blocked in Development.

**Phase 3: Validation**

- **Goal**: Confirm the auth endpoint behavior remains stable.
- **Scope**: Backend build and test commands.
- **Success signal**: `dotnet build server/FinancesControl.sln` and `dotnet test server/FinancesControl.sln` pass.

### Parallelism Notes

The phases should run sequentially because the tests depend on the final shape of the environment-aware mapping API.

---

## Decisions Log

| Decision | Choice | Alternatives | Rationale |
| -------- | ------ | ------------ | --------- |
| Register exposure | Development-only public `POST /register` | Keep blocked everywhere; build seed script; create production registration | Development-only access solves the immediate testing problem without changing product auth policy |
| Scope | Only register endpoint | Reopen all hidden Identity endpoints | The developer only needs user creation for local testing |
| Validation priority | Runtime endpoint tests | Swagger-only verification | Hidden or visible documentation does not guarantee authorization behavior |

---

## Research Summary

**Market Context**
This is an internal development workflow adjustment, not a market-facing feature. Market research is not applicable.

**Technical Context**
The backend currently maps ASP.NET Core Identity endpoints through `MapRestrictedIdentityApi<TUser>()`. That helper calls `MapIdentityApi<TUser>()`, inspects route patterns and HTTP methods, and applies a restrictive role requirement to anything outside the allow-list. `Program.cs` maps the helper and exposes Swagger only in Development, making the Identity mapping helper the natural implementation point for a Development-only register exception.

---

_Generated: 2026-07-19T16:22:00-03:00_
_Status: DRAFT - needs validation_
