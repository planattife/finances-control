using Dapper;

namespace FinancesControl.API.Transactions.Repository.SqlCommands;

public class TransactionExistsQuery
{
    public (string Sql, DynamicParameters Params) Build(long transactionId, long accountId)
    {
        var sql = @"
            SELECT EXISTS(
                SELECT 1 FROM ""Transactions""
                WHERE ""Id"" = @TransactionId AND ""AccountId"" = @AccountId
            )
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@TransactionId", transactionId);
        parameters.Add("@AccountId", accountId);

        return (sql, parameters);
    }
}
