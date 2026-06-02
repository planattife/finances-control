using Dapper;

namespace FinancesControl.API.Banks.Repository.SqlCommands;

public class BankExistsQuery
{
    public (string Sql, DynamicParameters Params) Build(long bankId, string userId)
    {
        var sql = @"
            SELECT EXISTS(
                SELECT 1 FROM ""Banks""
                WHERE ""Id"" = @BankId AND ""UserId"" = @UserId
            )
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@BankId", bankId);
        parameters.Add("@UserId", userId);

        return (sql, parameters);
    }
}
