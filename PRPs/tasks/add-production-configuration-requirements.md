# Add Production Configuration Requirements

## Where

`appsettings.json`, deployment configuration, `Program.cs`

## Impact

`DefaultConnection` and `Cors:AllowedOrigins` are not defined in committed config, and `AllowedHosts` is `"*"`. Missing or permissive production config can cause startup failure or broad host/CORS exposure.

## Expected Direction

Keep secrets outside source control, document required environment variables, fail fast when required config is missing, and set production `AllowedHosts`/CORS origins explicitly.
