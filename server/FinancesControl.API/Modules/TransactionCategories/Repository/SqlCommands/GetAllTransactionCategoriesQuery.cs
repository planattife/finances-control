using Dapper;

namespace FinancesControl.API.TransactionCategories.Repository.SqlCommands;

public class GetAllTransactionCategoriesQuery
{
    public (string Sql, DynamicParameters Params) Build(string? userId)
    {
        var sql = @"
            SELECT ""Id"", ""Name"", ""IsActive"", ""IsDefault"", ""UserId"", ""CreatedAt"", ""UpdatedAt""
            FROM ""TransactionCategories""
            WHERE ""IsDefault"" = true OR ""UserId"" = @UserId
            ORDER BY ""CreatedAt"" DESC;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId ?? (object)DBNull.Value);

        return (sql, parameters);
    }
}
