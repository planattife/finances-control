using Dapper;

namespace FinancesControl.API.Transactions.Repository.SqlCommands;

public class CountTransactionsByAccountQuery
{
    public (string Sql, DynamicParameters Params) Build(long accountId)
    {
        var sql = @"
            SELECT COUNT(*)
            FROM ""Transactions""
            WHERE ""AccountId"" = @AccountId;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@AccountId", accountId);

        return (sql, parameters);
    }
}
