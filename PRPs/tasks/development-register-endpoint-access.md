# Development Register Endpoint Access

## Source PRD

`PRPs/prds/development-register-endpoint-access.prd.md`

## Where

`server/FinancesControl.API/Common/IdentityEndpointRouteBuilderExtensions.cs`, `server/FinancesControl.API/Program.cs`, backend Identity endpoint tests

## Impact

The current restricted Identity endpoint mapping blocks `POST /register` in every environment. This protects production, but it also prevents the project developer from creating local test users during frontend development, making authenticated page testing harder.

## Expected Direction

Allow anonymous `POST /register` only when the API runs in the Development environment. Keep `POST /register` restricted in non-development environments, and keep all other hidden Identity endpoints restricted everywhere.

## Constraints

- Do not create a production public registration feature.
- Do not expose password reset, email confirmation, refresh, resend confirmation, 2FA, or profile mutation endpoints.
- Prefer runtime authorization/endpoint behavior tests over Swagger-only checks.
- No database schema, frontend UI, or Dapper repository changes are expected.
