using Dapper;
using FinancesControl.API.Common;

namespace FinancesControl.API.Transactions.Repository.SqlCommands;

public class GetAllTransactionsQuery
{
    public (string Sql, DynamicParameters Params) Build(long accountId, PaginationParams paginationParams)
    {
        var sql = @"
            SELECT ""Id"", ""TransactionDate"", ""Amount"", ""PaymentMethod"", ""TransactionType"", 
                   ""CategoryId"", ""AccountId"", ""IsActive"", ""BankId"", ""CardId"", 
                   ""CreatedAt"", ""UpdatedAt"", ""Description""
            FROM ""Transactions""
            WHERE ""AccountId"" = @AccountId
            ORDER BY ""CreatedAt"" DESC
            LIMIT @PageSize OFFSET @Offset;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@AccountId", accountId);
        parameters.Add("@PageSize", paginationParams.PageSize);
        parameters.Add("@Offset", paginationParams.GetOffset());

        return (sql, parameters);
    }
}
