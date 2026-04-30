using Dapper;
using FinancesControl.API.Models;

namespace FinancesControl.API.Cards.Repository.SqlCommands;

public class CreateCreditCardDetailsCommand
{
    public (string Sql, DynamicParameters Params) Build(CreditCard creditCard, long cardId)
    {
        var sql = @"
            INSERT INTO ""CreditCardDetails"" (""CardId"", ""ClosingDay"", ""DueDateDay"")
            VALUES (@CardId, @ClosingDay, @DueDateDay);
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@CardId", cardId);
        parameters.Add("@ClosingDay", creditCard.ClosingDay);
        parameters.Add("@DueDateDay", creditCard.DueDateDay);

        return (sql, parameters);
    }
}
