using Dapper;

namespace FinancesControl.API.Cards.Repository.SqlCommands;

public class GetCardByIdQuery
{
    public (string Sql, DynamicParameters Params) Build(long cardId, long accountId)
    {
        var sql = @"
            SELECT ""Id"", ""CardType"", ""CardNumber"", ""ExpirationDate"", ""IsActive"", 
                   ""CardNetwork"", ""BankId"", ""AccountId"", ""CardName"", ""CreatedAt"", ""UpdatedAt""
            FROM ""Cards""
            WHERE ""Id"" = @Id AND ""AccountId"" = @AccountId;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", cardId);
        parameters.Add("@AccountId", accountId);

        return (sql, parameters);
    }
}
