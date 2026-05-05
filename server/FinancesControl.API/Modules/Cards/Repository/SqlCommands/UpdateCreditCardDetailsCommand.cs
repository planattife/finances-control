using Dapper;
using FinancesControl.API.Models;

namespace FinancesControl.API.Cards.Repository.SqlCommands;

public class UpdateCreditCardDetailsCommand
{
    public (string Sql, DynamicParameters Params) Build(CreditCard creditCard)
    {
        var sql = @"
            UPDATE ""CreditCardDetails""
            SET ""ClosingDay"" = @ClosingDay, ""DueDateDay"" = @DueDateDay
            WHERE ""CardId"" = @CardId;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@CardId", creditCard.Id);
        parameters.Add("@ClosingDay", creditCard.ClosingDay);
        parameters.Add("@DueDateDay", creditCard.DueDateDay);

        return (sql, parameters);
    }
}
