using Dapper;

namespace FinancesControl.API.TransactionCategories.Repository.SqlCommands;

public class GetTransactionCategoryByIdQuery
{
    public (string Sql, DynamicParameters Params) Build(long categoryId, string? userId)
    {
        var sql = @"
            SELECT ""Id"", ""Name"", ""IsActive"", ""IsDefault"", ""UserId"", ""CreatedAt"", ""UpdatedAt""
            FROM ""TransactionCategories""
            WHERE ""Id"" = @Id 
            AND (""IsDefault"" = true OR ""UserId"" = @UserId);
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", categoryId);
        parameters.Add("@UserId", userId ?? (object)DBNull.Value);

        return (sql, parameters);
    }
}
