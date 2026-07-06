using Dapper;
using FinancesControl.API.Common;

namespace FinancesControl.API.Cards.Repository.SqlCommands;

public class GetAllCardsQuery
{
    public (string Sql, DynamicParameters Params) Build(long accountId, PaginationParams paginationParams)
    {
        var sql = @"
            SELECT c.""Id"", c.""CardType"", c.""CardNumber"", c.""ExpirationDate"", c.""IsActive"", 
                   c.""CardNetwork"", c.""BankId"", b.""Name"" AS ""BankName"", c.""AccountId"", 
                   c.""CardName"", c.""CreatedAt"", c.""UpdatedAt""
            FROM ""Cards"" c
            INNER JOIN ""Banks"" b ON c.""BankId"" = b.""Id""
            WHERE c.""AccountId"" = @AccountId
            ORDER BY c.""CreatedAt"" DESC
            LIMIT @PageSize OFFSET @Offset;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@AccountId", accountId);
        parameters.Add("@PageSize", paginationParams.PageSize);
        parameters.Add("@Offset", paginationParams.GetOffset());

        return (sql, parameters);
    }
}
