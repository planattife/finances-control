using Dapper;

namespace FinancesControl.API.Transactions.Repository.SqlCommands;

public class GetTransactionByIdQuery
{
    public (string Sql, DynamicParameters Params) Build(long transactionId)
    {
        var sql = @"
            SELECT ""Id"", ""TransactionDate"", ""Amount"", ""PaymentMethod"", ""TransactionType"", 
                   ""CategoryId"", ""AccountId"", ""IsActive"", ""BankId"", ""CardId"", 
                   ""CreatedAt"", ""UpdatedAt"", ""Description""
            FROM ""Transactions""
            WHERE ""Id"" = @Id;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", transactionId);

        return (sql, parameters);
    }
}
