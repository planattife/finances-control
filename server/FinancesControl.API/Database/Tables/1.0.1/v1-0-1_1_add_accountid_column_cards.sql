DO $$
DECLARE
    has_null_card_account_id BOOLEAN;
BEGIN
    ALTER TABLE "Cards" ADD COLUMN IF NOT EXISTS "AccountId" BIGINT;
    IF EXISTS (
        SELECT 1
        FROM "Transactions"
        WHERE "CardId" IS NOT NULL
        GROUP BY "CardId"
        HAVING COUNT(DISTINCT "AccountId") > 1
    ) THEN
        RAISE EXCEPTION 'Cards.AccountId backfill failed: at least one card has transactions in multiple accounts.';
    END IF;
    EXECUTE 'WITH "CardAccounts" AS (
        SELECT "CardId", MIN("AccountId") AS "AccountId"
        FROM "Transactions"
        WHERE "CardId" IS NOT NULL
        GROUP BY "CardId"
        HAVING COUNT(DISTINCT "AccountId") = 1
    )
    UPDATE "Cards" c
    SET "AccountId" = "CardAccounts"."AccountId"
    FROM "CardAccounts"
    WHERE c."Id" = "CardAccounts"."CardId"
    AND c."AccountId" IS NULL';
    EXECUTE 'SELECT EXISTS (SELECT 1 FROM "Cards" WHERE "AccountId" IS NULL)' INTO has_null_card_account_id;
    IF has_null_card_account_id THEN
        RAISE EXCEPTION 'Cards.AccountId backfill failed: at least one card has no deterministic account mapping.';
    END IF;
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'FK_Cards_Accounts_AccountId'
        AND conrelid = '"Cards"'::regclass
    ) THEN
        EXECUTE 'ALTER TABLE "Cards" ADD CONSTRAINT "FK_Cards_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts"("Id") ON DELETE RESTRICT NOT VALID';
    END IF;
    EXECUTE 'ALTER TABLE "Cards" VALIDATE CONSTRAINT "FK_Cards_Accounts_AccountId"';
    EXECUTE 'ALTER TABLE "Cards" ALTER COLUMN "AccountId" SET NOT NULL';
END $$;
