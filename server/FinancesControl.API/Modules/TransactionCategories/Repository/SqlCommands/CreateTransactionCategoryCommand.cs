using Dapper;
using FinancesControl.API.Models;

namespace FinancesControl.API.TransactionCategories.Repository.SqlCommands;

public class CreateTransactionCategoryCommand
{
    public (string Sql, DynamicParameters Params) Build(TransactionCategory category)
    {
        var sql = @"
            INSERT INTO ""TransactionCategories"" (""Name"", ""IsActive"", ""IsDefault"", ""UserId"", ""CreatedAt"")
            VALUES (@Name, @IsActive, @IsDefault, @UserId, @CreatedAt)
            RETURNING ""Id"";
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Name", category.Name);
        parameters.Add("@IsActive", category.IsActive);
        parameters.Add("@IsDefault", category.IsDefault);
        parameters.Add("@UserId", category.UserId ?? (object)DBNull.Value);
        parameters.Add("@CreatedAt", DateTime.UtcNow);

        return (sql, parameters);
    }
}
