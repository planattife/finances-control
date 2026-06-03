using Dapper;

namespace FinancesControl.API.Accounts.Repository.SqlCommands;

public class CountAccountsByUserQuery
{
    public (string Sql, DynamicParameters Params) Build(string userId)
    {
        var sql = @"
            SELECT COUNT(*)
            FROM ""Accounts""
            WHERE ""UserId"" = @UserId;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);

        return (sql, parameters);
    }
}
