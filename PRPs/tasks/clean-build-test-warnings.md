# Clean Build/Test Warnings Before Release

## Where

API response DTOs, transaction DTOs, tests under `server/FinancesControl.Tests`

## Impact

The backend builds and tests pass, but many nullable and unawaited-call warnings can hide real defects and weaken CI signal.

## Expected Direction

Make nullable response payloads explicit, initialize DTO properties or convert to records, and await all async substitute/test calls. Consider treating warnings as errors in CI after cleanup.
