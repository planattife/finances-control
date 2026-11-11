# Add API Rate Limiting

## Where

`Program.cs`

## Impact

Login, registration, and finance write endpoints have no app-level request throttling, increasing brute-force and abuse risk.

## Expected Direction

Use ASP.NET Core rate limiting policies. Apply stricter limits to auth endpoints and reasonable per-user/per-IP limits to authenticated API routes.
