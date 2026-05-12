using Dapper;

namespace FinancesControl.API.Accounts.Repository.SqlCommands;

public class GetAccountByIdQuery
{
    public (string Sql, DynamicParameters Params) Build(long accountId, string userId)
    {
        var sql = @"
            SELECT ""Id"", ""Name"", ""UserId"", ""IsActive"", ""CreatedAt"", ""UpdatedAt""
            FROM ""Accounts""
            WHERE ""Id"" = @Id AND ""UserId"" = @UserId;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", accountId);
        parameters.Add("@UserId", userId);

        return (sql, parameters);
    }
}
