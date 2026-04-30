using Dapper;

namespace FinancesControl.API.Cards.Repository.SqlCommands;

public class GetCardByIdQuery
{
    public (string Sql, DynamicParameters Params) Build(long cardId)
    {
        var sql = @"
            SELECT ""Id"", ""CardType"", ""CardNumber"", ""ExpirationDate"", ""IsActive"", 
                   ""CardNetwork"", ""BankId"", ""CardName"", ""CreatedAt"", ""UpdatedAt""
            FROM ""Cards""
            WHERE ""Id"" = @Id;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", cardId);

        return (sql, parameters);
    }
}
