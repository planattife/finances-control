using Dapper;

namespace FinancesControl.API.Cards.Repository.SqlCommands;

public class CardExistsQuery
{
    public (string Sql, DynamicParameters Params) Build(long cardId, long accountId)
    {
        var sql = @"
            SELECT EXISTS(
                SELECT 1 FROM ""Cards""
                WHERE ""Id"" = @CardId AND ""AccountId"" = @AccountId
            )
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@CardId", cardId);
        parameters.Add("@AccountId", accountId);

        return (sql, parameters);
    }
}
