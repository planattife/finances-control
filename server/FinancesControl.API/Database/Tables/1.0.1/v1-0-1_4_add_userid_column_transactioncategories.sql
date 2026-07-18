DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'TransactionCategories' 
        AND column_name = 'UserId'
    ) THEN
        ALTER TABLE "TransactionCategories" 
        ADD COLUMN "UserId" TEXT NULL;
        
        -- Add foreign key constraint
        ALTER TABLE "TransactionCategories"
        ADD CONSTRAINT FK_TransactionCategories_AspNetUsers_UserId 
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE;
    END IF;
END
$$;
