using Dapper;

namespace FinancesControl.API.Accounts.Repository.SqlCommands;

public class GetAccountByIdQuery
{
    public (string Sql, DynamicParameters Params) Build(long accountId)
    {
        var sql = @"
            SELECT ""Id"", ""Name"", ""UserId"", ""IsActive"", ""CreatedAt"", ""UpdatedAt""
            FROM ""Accounts""
            WHERE ""Id"" = @Id;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", accountId);

        return (sql, parameters);
    }
}
