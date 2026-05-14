using Dapper;

namespace FinancesControl.API.Cards.Repository.SqlCommands;

public class GetCreditCardByIdQuery
{
    public (string Sql, DynamicParameters Params) Build(long cardId, long accountId)
    {
        var sql = @"
            SELECT c.""Id"", c.""CardType"", c.""CardNumber"", c.""ExpirationDate"", c.""IsActive"", 
                   c.""CardNetwork"", c.""BankId"", c.""AccountId"", c.""CardName"", c.""CreatedAt"", c.""UpdatedAt"",
                   ccd.""ClosingDay"", ccd.""DueDateDay""
            FROM ""Cards"" c
            LEFT JOIN ""CreditCardDetails"" ccd ON c.""Id"" = ccd.""CardId""
            WHERE c.""Id"" = @Id AND c.""AccountId"" = @AccountId AND c.""CardType"" = @CardType;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", cardId);
        parameters.Add("@AccountId", accountId);
        parameters.Add("@CardType", 1);

        return (sql, parameters);
    }
}
