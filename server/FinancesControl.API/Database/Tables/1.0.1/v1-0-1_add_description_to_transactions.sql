DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'Transactions' AND column_name = 'Description'
    ) THEN
        ALTER TABLE "Transactions"
        ADD COLUMN "Description" VARCHAR(500) NULL;
    END IF;
END $$;
