# Scope Transaction Category Ownership In Transaction Validation

## Where

`TransactionService.ValidateForeignKeys`, `TransactionService.Update`, `TransactionCategoryService.ExistsAsync`, `TransactionCategoryRepository.ExistsAsync`, `TransactionCategoryExistsQuery`

## Impact

Transactions can reference another user's custom category by ID because category existence is checked globally instead of by current user/default category visibility.

## Expected Direction

Change category existence checks to accept `userId` and return true only for default categories or categories owned by that user. Update tests for cross-user category rejection.
