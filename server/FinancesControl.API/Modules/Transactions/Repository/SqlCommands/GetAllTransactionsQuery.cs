namespace FinancesControl.API.Transactions.Repository.SqlCommands;

public class GetAllTransactionsQuery
{
    public string Sql => @"
        SELECT ""Id"", ""TransactionDate"", ""Amount"", ""PaymentMethod"", ""TransactionType"", 
               ""CategoryId"", ""AccountId"", ""IsActive"", ""BankId"", ""CardId"", 
               ""CreatedAt"", ""UpdatedAt""
        FROM ""Transactions""
        ORDER BY ""CreatedAt"" DESC;
    ";
}
