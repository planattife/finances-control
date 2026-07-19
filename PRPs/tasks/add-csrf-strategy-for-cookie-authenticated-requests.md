# Add CSRF Strategy For Cookie Authenticated Requests

## Where

`src/api/axiosClient.ts`, `src/features/auth/authApi.ts`, all data-changing API calls.

## Impact

The frontend uses cookie-based auth with `withCredentials: true` and `/login?useCookies=true`. If the backend accepts cross-site cookie requests without an anti-forgery token strategy, authenticated users can be exposed to CSRF attacks.

## Expected Direction

Align with the backend on the intended CSRF protection. If the backend issues an anti-forgery token, read it from the documented source and attach it in the shared Axios client for `POST`, `PUT`, `PATCH`, and `DELETE` requests. If SameSite-only protection is the chosen approach, document and verify the cookie/CORS settings for production.
