# Fix Database Migration Scripts For Production Safety And Conventions

## Where

`Database/Tables/1.0.0/v1-0-0_create_credit_card_details_table.sql`, `Database/Tables/1.0.1/v1-0-1_add_accountid_column_cards.sql`

## Impact

Adding `Cards.AccountId BIGINT NOT NULL` without default/backfill can fail on databases with existing rows.

## Expected Direction

Split the card account migration into nullable add, deterministic backfill, constraint validation, then `SET NOT NULL`.
