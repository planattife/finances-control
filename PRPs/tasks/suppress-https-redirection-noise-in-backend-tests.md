# Suppress HTTPS Redirection Noise In Backend Tests

## Where

`Program.cs`, backend integration test host setup using `WebApplicationFactory`

## Impact

Backend tests log repeated `HttpsRedirectionMiddleware` warnings because the test host does not expose a real HTTPS endpoint or configured HTTPS port. The warnings do not fail tests, but they make test output noisy and can hide meaningful warnings.

## Expected Direction

Configure tests to run under a dedicated environment such as `Testing`, and skip `app.UseHttpsRedirection()` for that environment. Alternatively, configure `HttpsRedirection:HttpsPort` in test configuration if redirect behavior must stay active in tests.

## Constraints

Leave EF Core InMemory save logs alone unless broader test logging cleanup is planned.
