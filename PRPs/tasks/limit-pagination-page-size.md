# Limit Pagination Page Size

## Where

`Common/PaginationParams.cs`; list endpoints for accounts, banks, cards, categories, and transactions.

## Impact

Any authenticated user can request an arbitrarily large `PageSize`, which can cause high memory/CPU/database load in production.

## Expected Direction

Add a documented maximum, for example `MaxPageSize = 100`, and clamp positive values above the max. Add tests for negative, zero, default, and oversized page sizes.
