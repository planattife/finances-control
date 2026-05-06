using Dapper;
using FinancesControl.API.Models;

namespace FinancesControl.API.Transactions.Repository.SqlCommands;

public class UpdateTransactionCommand
{
    public (string Sql, DynamicParameters Params) Build(Transaction transaction)
    {
        var sql = @"
            UPDATE ""Transactions""
            SET ""TransactionDate"" = @TransactionDate, ""Amount"" = @Amount, ""PaymentMethod"" = @PaymentMethod, 
                ""TransactionType"" = @TransactionType, ""CategoryId"" = @CategoryId, ""IsActive"" = @IsActive, 
                ""UpdatedAt"" = @UpdatedAt, ""Description"" = @Description
            WHERE ""Id"" = @Id;
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@Id", transaction.Id);
        parameters.Add("@TransactionDate", transaction.TransactionDate);
        parameters.Add("@Amount", transaction.Amount);
        parameters.Add("@PaymentMethod", (int)transaction.PaymentMethod);
        parameters.Add("@TransactionType", (int)transaction.TransactionType);
        parameters.Add("@CategoryId", transaction.CategoryId);
        parameters.Add("@IsActive", transaction.IsActive);
        parameters.Add("@UpdatedAt", transaction.UpdatedAt);
        parameters.Add("@Description", transaction.Description);

        return (sql, parameters);
    }
}
