using Dapper;

namespace FinancesControl.API.Transactions.Repository.SqlCommands;

public class GetTransactionByIdQuery
{
    public (string Sql, DynamicParameters Params) Build(long transactionId, long accountId)
    {
        var sql = @"
            SELECT t.""Id"", t.""TransactionDate"", t.""Amount"", t.""PaymentMethod"", t.""TransactionType"", 
                   t.""CategoryId"", c.""Name"" AS ""CategoryName"", t.""AccountId"", t.""IsActive"", 
                   t.""BankId"", t.""CardId"", t.""CreatedAt"", t.""UpdatedAt"", t.""Description""
            FROM ""Transactions"" t
            INNER JOIN ""TransactionCategories"" c ON t.""CategoryId"" = c.""Id""
            WHERE t.""Id"" = @Id AND t.""AccountId"" = @AccountId;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", transactionId);
        parameters.Add("@AccountId", accountId);

        return (sql, parameters);
    }
}
