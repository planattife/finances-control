# Review And Restrict Public Authentication Endpoints

## Where

`Program.cs`, `app.MapIdentityApi<IdentityUser>()`

## Impact

The app exposes default Identity API endpoints. Production may unintentionally allow public registration or unsupported auth flows.

## Expected Direction

Decide which Identity endpoints are allowed in production. If registration must be controlled, replace or wrap default endpoints with explicit auth controllers/policies.
