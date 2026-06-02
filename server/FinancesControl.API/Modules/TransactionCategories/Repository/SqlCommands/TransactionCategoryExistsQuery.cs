using Dapper;

namespace FinancesControl.API.TransactionCategories.Repository.SqlCommands;

public class TransactionCategoryExistsQuery
{
    public (string Sql, DynamicParameters Params) Build(long categoryId)
    {
        var sql = @"
            SELECT EXISTS(
                SELECT 1 FROM ""TransactionCategories""
                WHERE ""Id"" = @CategoryId
            )
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@CategoryId", categoryId);

        return (sql, parameters);
    }
}
