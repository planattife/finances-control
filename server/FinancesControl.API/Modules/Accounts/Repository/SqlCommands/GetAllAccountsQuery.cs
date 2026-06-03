using Dapper;

namespace FinancesControl.API.Accounts.Repository.SqlCommands;

public class GetAllAccountsQuery
{
    public (string Sql, DynamicParameters Params) Build(string userId)
    {
        var sql = @"
            SELECT ""Id"", ""Name"", ""UserId"", ""IsActive"", ""CreatedAt"", ""UpdatedAt""
            FROM ""Accounts""
            WHERE ""UserId"" = @UserId
            ORDER BY ""CreatedAt"" DESC;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);

        return (sql, parameters);
    }
}
