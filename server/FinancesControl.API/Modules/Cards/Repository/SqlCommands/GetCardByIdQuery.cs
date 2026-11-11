using Dapper;

namespace FinancesControl.API.Cards.Repository.SqlCommands;

public class GetCardByIdQuery
{
    public (string Sql, DynamicParameters Params) Build(long cardId, long accountId)
    {
        var sql = @"
            SELECT c.""Id"", c.""CardType"", c.""CardNumber"", c.""ExpirationDate"", c.""IsActive"", 
                   c.""CardNetwork"", c.""BankId"", b.""Name"" AS ""BankName"", c.""AccountId"", 
                   c.""CardName"", c.""CreatedAt"", c.""UpdatedAt""
            FROM ""Cards"" c
            INNER JOIN ""Banks"" b ON c.""BankId"" = b.""Id""
            WHERE c.""Id"" = @Id AND c.""AccountId"" = @AccountId;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", cardId);
        parameters.Add("@AccountId", accountId);

        return (sql, parameters);
    }
}
