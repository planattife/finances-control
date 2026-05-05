using Dapper;
using FinancesControl.API.Models;

namespace FinancesControl.API.TransactionCategories.Repository.SqlCommands;

public class CreateTransactionCategoryCommand
{
    public (string Sql, DynamicParameters Params) Build(TransactionCategory category)
    {
        var sql = @"
            INSERT INTO ""TransactionCategories"" (""Name"", ""IsActive"", ""CreatedAt"")
            VALUES (@Name, @IsActive, @CreatedAt)
            RETURNING ""Id"";
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Name", category.Name);
        parameters.Add("@IsActive", category.IsActive);
        parameters.Add("@CreatedAt", DateTime.UtcNow);

        return (sql, parameters);
    }
}
