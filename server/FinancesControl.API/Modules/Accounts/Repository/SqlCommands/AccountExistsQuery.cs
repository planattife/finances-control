using Dapper;

namespace FinancesControl.API.Accounts.Repository.SqlCommands;

public class AccountExistsQuery
{
    public (string Sql, DynamicParameters Params) Build(long accountId, string userId)
    {
        var sql = @"
            SELECT EXISTS(
                SELECT 1 FROM ""Accounts"" 
                WHERE ""Id"" = @AccountId AND ""UserId"" = @UserId
            )
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@AccountId", accountId);
        parameters.Add("@UserId", userId);

        return (sql, parameters);
    }
}
