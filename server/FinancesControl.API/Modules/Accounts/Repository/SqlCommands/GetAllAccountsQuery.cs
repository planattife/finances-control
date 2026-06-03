using Dapper;
using FinancesControl.API.Common;

namespace FinancesControl.API.Accounts.Repository.SqlCommands;

public class GetAllAccountsQuery
{
    public (string Sql, DynamicParameters Params) Build(string userId, PaginationParams paginationParams)
    {
        var sql = @"
            SELECT ""Id"", ""Name"", ""UserId"", ""IsActive"", ""CreatedAt"", ""UpdatedAt""
            FROM ""Accounts""
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
