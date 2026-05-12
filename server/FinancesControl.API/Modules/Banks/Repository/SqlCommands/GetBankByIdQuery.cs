using Dapper;

namespace FinancesControl.API.Banks.Repository.SqlCommands;

public class GetBankByIdQuery
{
    public (string Sql, DynamicParameters Params) Build(long bankId, string userId)
    {
        var sql = @"
            SELECT ""Id"", ""Name"", ""AccountType"", ""IsActive"", ""UserId"", ""CreatedAt"", ""UpdatedAt""
            FROM ""Banks""
            WHERE ""Id"" = @Id AND ""UserId"" = @UserId;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", bankId);
        parameters.Add("@UserId", userId);

        return (sql, parameters);
    }
}
