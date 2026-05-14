using Dapper;

namespace FinancesControl.API.Transactions.Repository.SqlCommands;

public class GetTransactionByIdQuery
{
    public (string Sql, DynamicParameters Params) Build(long transactionId, long accountId)
    {
        var sql = @"
            SELECT ""Id"", ""TransactionDate"", ""Amount"", ""PaymentMethod"", ""TransactionType"", 
                   ""CategoryId"", ""AccountId"", ""IsActive"", ""BankId"", ""CardId"", 
                   ""CreatedAt"", ""UpdatedAt"", ""Description""
            FROM ""Transactions""
            WHERE ""Id"" = @Id AND ""AccountId"" = @AccountId;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", transactionId);
        parameters.Add("@AccountId", accountId);

        return (sql, parameters);
    }
}
