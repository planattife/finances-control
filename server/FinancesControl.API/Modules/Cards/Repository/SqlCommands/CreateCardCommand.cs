using Dapper;
using FinancesControl.API.Models;

namespace FinancesControl.API.Cards.Repository.SqlCommands;

public class CreateCardCommand
{
    public (string Sql, DynamicParameters Params) Build(Card card)
    {
        var sql = @"
            INSERT INTO ""Cards"" (""CardType"", ""CardNumber"", ""ExpirationDate"", ""IsActive"", ""CardNetwork"", ""BankId"", ""CardName"", ""CreatedAt"")
            VALUES (@CardType, @CardNumber, @ExpirationDate, @IsActive, @CardNetwork, @BankId, @CardName, @CreatedAt)
            RETURNING ""Id"";
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@CardType", (int)card.CardType);
        parameters.Add("@CardNumber", card.CardNumber);
        parameters.Add("@ExpirationDate", card.ExpirationDate);
        parameters.Add("@IsActive", card.IsActive);
        parameters.Add("@CardNetwork", (int)card.CardNetwork);
        parameters.Add("@BankId", card.BankId);
        parameters.Add("@CardName", card.CardName);
        parameters.Add("@CreatedAt", card.CreatedAt);

        return (sql, parameters);
    }
}
