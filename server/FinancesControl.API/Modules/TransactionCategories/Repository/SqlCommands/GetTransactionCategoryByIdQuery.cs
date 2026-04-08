using Dapper;

namespace FinancesControl.API.TransactionCategories.Repository.SqlCommands;

public class GetTransactionCategoryByIdQuery
{
    public (string Sql, DynamicParameters Params) Build(long categoryId)
    {
        var sql = @"
            SELECT ""Id"", ""Name"", ""IsActive"", ""CreatedAt"", ""UpdatedAt""
            FROM ""TransactionCategories""
            WHERE ""Id"" = @Id;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", categoryId);

        return (sql, parameters);
    }
}
