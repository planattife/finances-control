using Dapper;
using FinancesControl.API.Models;

namespace FinancesControl.API.Cards.Repository.SqlCommands;

public class UpdateCardCommand
{
    public (string Sql, DynamicParameters Params) Build(Card card, long accountId)
    {
        var sql = @"
            UPDATE ""Cards""
            SET ""CardType"" = @CardType, ""CardNumber"" = @CardNumber, ""ExpirationDate"" = @ExpirationDate, 
                ""IsActive"" = @IsActive, ""CardNetwork"" = @CardNetwork, ""CardName"" = @CardName, ""UpdatedAt"" = @UpdatedAt
            WHERE ""Id"" = @Id AND ""AccountId"" = @AccountId;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", card.Id);
        parameters.Add("@AccountId", accountId);
        parameters.Add("@CardType", (int)card.CardType);
        parameters.Add("@CardNumber", card.CardNumber);
        parameters.Add("@ExpirationDate", card.ExpirationDate);
        parameters.Add("@IsActive", card.IsActive);
        parameters.Add("@CardNetwork", (int)card.CardNetwork);
        parameters.Add("@CardName", card.CardName);
        parameters.Add("@UpdatedAt", card.UpdatedAt);

        return (sql, parameters);
    }
}
