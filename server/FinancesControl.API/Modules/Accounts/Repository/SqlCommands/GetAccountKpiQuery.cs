using Dapper;

namespace FinancesControl.API.Accounts.Repository.SqlCommands;

public class GetAccountKpiQuery
{
    public (string Sql, DynamicParameters Params) Build(long accountId, string userId)
    {
        var sql = @"
            SELECT
                a.""Id"" AS ""AccountId"",
                COALESCE(c.""CardsCount"", 0)::int AS ""CardsCount"",
                COALESCE(t.""TransactionsCount"", 0)::int AS ""TransactionsCount""
            FROM ""Accounts"" a
            LEFT JOIN (
                SELECT ""AccountId"", COUNT(*) AS ""CardsCount""
                FROM ""Cards""
                GROUP BY ""AccountId""
            ) c ON c.""AccountId"" = a.""Id""
            LEFT JOIN (
                SELECT ""AccountId"", COUNT(*) AS ""TransactionsCount""
                FROM ""Transactions""
                GROUP BY ""AccountId""
            ) t ON t.""AccountId"" = a.""Id""
            WHERE a.""Id"" = @AccountId
              AND a.""UserId"" = @UserId;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@AccountId", accountId);
        parameters.Add("@UserId", userId);

        return (sql, parameters);
    }
}
