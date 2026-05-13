using Dapper;

namespace FinancesControl.API.Cards.Repository.SqlCommands;

public class GetAllCardsQuery
{
    public (string Sql, DynamicParameters Params) Build(long accountId)
    {
        var sql = @"
            SELECT ""Id"", ""CardType"", ""CardNumber"", ""ExpirationDate"", ""IsActive"", 
                   ""CardNetwork"", ""BankId"", ""AccountId"", ""CardName"", ""CreatedAt"", ""UpdatedAt""
            FROM ""Cards""
            WHERE ""AccountId"" = @AccountId
            ORDER BY ""CreatedAt"" DESC;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@AccountId", accountId);

        return (sql, parameters);
    }
}
