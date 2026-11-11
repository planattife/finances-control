using Dapper;

namespace FinancesControl.API.TransactionCategories.Repository.SqlCommands;

public class TransactionCategoryExistsQuery
{
    public (string Sql, DynamicParameters Params) Build(long categoryId, string? userId)
    {
        var sql = @"
            SELECT EXISTS(
                SELECT 1 FROM ""TransactionCategories""
                WHERE ""Id"" = @CategoryId
                AND (""IsDefault"" = true OR ""UserId"" = @UserId)
            )
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@CategoryId", categoryId);
        parameters.Add("@UserId", userId ?? (object)DBNull.Value);

        return (sql, parameters);
    }
}
