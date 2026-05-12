using Dapper;

namespace FinancesControl.API.Banks.Repository.SqlCommands;

public class GetAllBanksQuery
{
    public (string Sql, DynamicParameters Params) Build(string userId)
    {
        var sql = @"
            SELECT ""Id"", ""Name"", ""AccountType"", ""IsActive"", ""UserId"", ""CreatedAt"", ""UpdatedAt""
            FROM ""Banks""
            WHERE ""UserId"" = @UserId
            ORDER BY ""CreatedAt"" DESC;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);

        return (sql, parameters);
    }
}
