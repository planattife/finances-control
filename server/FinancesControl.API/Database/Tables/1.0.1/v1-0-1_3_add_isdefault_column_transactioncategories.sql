DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'TransactionCategories' 
        AND column_name = 'IsDefault'
    ) THEN
        ALTER TABLE "TransactionCategories" 
        ADD COLUMN "IsDefault" BOOLEAN NOT NULL DEFAULT false;
    END IF;
END
$$;
