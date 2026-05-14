using Dapper;

namespace FinancesControl.API.Transactions.Repository.SqlCommands;

public class GetAllTransactionsQuery
{
    public (string Sql, DynamicParameters Params) Build(long accountId)
    {
        var sql = @"
            SELECT ""Id"", ""TransactionDate"", ""Amount"", ""PaymentMethod"", ""TransactionType"", 
                   ""CategoryId"", ""AccountId"", ""IsActive"", ""BankId"", ""CardId"", 
                   ""CreatedAt"", ""UpdatedAt""
            FROM ""Transactions""
            WHERE ""AccountId"" = @AccountId
            ORDER BY ""CreatedAt"" DESC;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@AccountId", accountId);

        return (sql, parameters);
    }
}
