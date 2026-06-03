using Dapper;
using FinancesControl.API.Common;

namespace FinancesControl.API.Cards.Repository.SqlCommands;

public class GetAllCardsQuery
{
    public (string Sql, DynamicParameters Params) Build(long accountId, PaginationParams paginationParams)
    {
        var sql = @"
            SELECT ""Id"", ""CardType"", ""CardNumber"", ""ExpirationDate"", ""IsActive"", 
                   ""CardNetwork"", ""BankId"", ""AccountId"", ""CardName"", ""CreatedAt"", ""UpdatedAt""
            FROM ""Cards""
            WHERE ""AccountId"" = @AccountId
            ORDER BY ""CreatedAt"" DESC
            LIMIT @PageSize OFFSET @Offset;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@AccountId", accountId);
        parameters.Add("@PageSize", paginationParams.PageSize);
        parameters.Add("@Offset", paginationParams.GetOffset());

        return (sql, parameters);
    }
}
