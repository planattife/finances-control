namespace FinancesControl.API.TransactionCategories.Repository.SqlCommands;

public class GetAllTransactionCategoriesQuery
{
    public string Sql => @"
        SELECT ""Id"", ""Name"", ""IsActive"", ""CreatedAt"", ""UpdatedAt""
        FROM ""TransactionCategories""
        ORDER BY ""CreatedAt"" DESC;
    ";
}
