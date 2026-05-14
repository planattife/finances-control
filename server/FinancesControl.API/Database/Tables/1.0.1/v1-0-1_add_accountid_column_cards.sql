DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'Cards' 
        AND column_name = 'AccountId'
    ) THEN
        ALTER TABLE "Cards" 
        ADD COLUMN "AccountId" BIGINT NOT NULL;
        
        ALTER TABLE "Cards" 
        ADD CONSTRAINT "FK_Cards_Accounts_AccountId" 
        FOREIGN KEY ("AccountId") REFERENCES "Accounts"("Id") ON DELETE RESTRICT;
    END IF;
END $$;
