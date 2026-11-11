using Dapper;

namespace FinancesControl.API.Cards.Repository.SqlCommands;

public class CountCardsByAccountQuery
{
    public (string Sql, DynamicParameters Params) Build(long accountId)
    {
        var sql = @"
            SELECT COUNT(*)
            FROM ""Cards""
            WHERE ""AccountId"" = @AccountId;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@AccountId", accountId);

        return (sql, parameters);
    }
}
