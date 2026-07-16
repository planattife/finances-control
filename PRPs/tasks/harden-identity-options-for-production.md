# Harden Identity Options For Production

## Where

`Program.cs`, `AddIdentityApiEndpoints<IdentityUser>()`

## Impact

Password, lockout, sign-in, and user options rely on framework defaults, which may not match the security posture expected for a production finance app.

## Expected Direction

Configure explicit `IdentityOptions`: password length/complexity, unique email, lockout thresholds, and confirmed-account policy if email confirmation will be used.
