using Dapper;
using FinancesControl.API.Common;

namespace FinancesControl.API.Transactions.Repository.SqlCommands;

public class GetAllTransactionsQuery
{
    public (string Sql, DynamicParameters Params) Build(long accountId, PaginationParams paginationParams)
    {
        var sql = @"
            SELECT t.""Id"", t.""TransactionDate"", t.""Amount"", t.""PaymentMethod"", t.""TransactionType"", 
                   t.""CategoryId"", c.""Name"" AS ""CategoryName"", t.""AccountId"", t.""IsActive"", 
                   t.""BankId"", t.""CardId"", t.""CreatedAt"", t.""UpdatedAt"", t.""Description""
            FROM ""Transactions"" t
            INNER JOIN ""TransactionCategories"" c ON t.""CategoryId"" = c.""Id""
            WHERE t.""AccountId"" = @AccountId
              AND t.""IsActive"" = TRUE
            ORDER BY t.""CreatedAt"" DESC
            LIMIT @PageSize OFFSET @Offset;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@AccountId", accountId);
        parameters.Add("@PageSize", paginationParams.PageSize);
        parameters.Add("@Offset", paginationParams.GetOffset());

        return (sql, parameters);
    }
}
