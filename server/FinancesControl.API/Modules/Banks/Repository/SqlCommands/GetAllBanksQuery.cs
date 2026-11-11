using Dapper;
using FinancesControl.API.Common;

namespace FinancesControl.API.Banks.Repository.SqlCommands;

public class GetAllBanksQuery
{
    public (string Sql, DynamicParameters Params) Build(string userId, PaginationParams paginationParams)
    {
        var sql = @"
            SELECT ""Id"", ""Name"", ""AccountType"", ""IsActive"", ""UserId"", ""CreatedAt"", ""UpdatedAt""
            FROM ""Banks""
            WHERE ""UserId"" = @UserId
            ORDER BY ""CreatedAt"" DESC
            LIMIT @PageSize OFFSET @Offset;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@PageSize", paginationParams.PageSize);
        parameters.Add("@Offset", paginationParams.GetOffset());

        return (sql, parameters);
    }
}
