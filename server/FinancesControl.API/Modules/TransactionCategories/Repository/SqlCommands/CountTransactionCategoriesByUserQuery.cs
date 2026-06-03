using Dapper;

namespace FinancesControl.API.TransactionCategories.Repository.SqlCommands;

public class CountTransactionCategoriesByUserQuery
{
    public (string Sql, DynamicParameters Params) Build(string? userId)
    {
        var sql = @"
            SELECT COUNT(*)
            FROM ""TransactionCategories""
            WHERE ""IsDefault"" = true OR ""UserId"" = @UserId;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId ?? (object)DBNull.Value);

        return (sql, parameters);
    }
}
