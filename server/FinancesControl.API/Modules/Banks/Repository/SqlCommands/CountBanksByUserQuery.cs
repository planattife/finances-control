using Dapper;

namespace FinancesControl.API.Banks.Repository.SqlCommands;

public class CountBanksByUserQuery
{
    public (string Sql, DynamicParameters Params) Build(string userId)
    {
        var sql = @"
            SELECT COUNT(*)
            FROM ""Banks""
            WHERE ""UserId"" = @UserId;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);

        return (sql, parameters);
    }
}
