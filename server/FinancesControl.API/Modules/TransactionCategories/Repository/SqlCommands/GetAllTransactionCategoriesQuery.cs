using Dapper;
using FinancesControl.API.Common;

namespace FinancesControl.API.TransactionCategories.Repository.SqlCommands;

public class GetAllTransactionCategoriesQuery
{
    public (string Sql, DynamicParameters Params) Build(string? userId, PaginationParams paginationParams)
    {
        var sql = @"
            SELECT ""Id"", ""Name"", ""IsActive"", ""IsDefault"", ""UserId"", ""CreatedAt"", ""UpdatedAt""
            FROM ""TransactionCategories""
            WHERE ""IsDefault"" = true OR ""UserId"" = @UserId
            ORDER BY ""CreatedAt"" DESC
            LIMIT @PageSize OFFSET @Offset;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId ?? (object)DBNull.Value);
        parameters.Add("@PageSize", paginationParams.PageSize);
        parameters.Add("@Offset", paginationParams.GetOffset());

        return (sql, parameters);
    }
}
