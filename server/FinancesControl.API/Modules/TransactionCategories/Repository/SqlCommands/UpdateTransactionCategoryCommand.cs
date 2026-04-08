using Dapper;
using FinancesControl.API.Models;

namespace FinancesControl.API.TransactionCategories.Repository.SqlCommands;

public class UpdateTransactionCategoryCommand
{
    public (string Sql, DynamicParameters Params) Build(TransactionCategory category)
    {
        var sql = @"
            UPDATE ""TransactionCategories""
            SET ""Name"" = @Name, ""IsActive"" = @IsActive, ""UpdatedAt"" = @UpdatedAt
            WHERE ""Id"" = @Id;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", category.Id);
        parameters.Add("@Name", category.Name);
        parameters.Add("@IsActive", category.IsActive);
        parameters.Add("@UpdatedAt", DateTime.UtcNow);

        return (sql, parameters);
    }
}
