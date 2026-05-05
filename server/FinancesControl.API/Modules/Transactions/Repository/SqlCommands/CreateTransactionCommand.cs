using Dapper;
using FinancesControl.API.Models;

namespace FinancesControl.API.Transactions.Repository.SqlCommands;

public class CreateTransactionCommand
{
    public (string Sql, DynamicParameters Params) Build(Transaction transaction)
    {
        var sql = @"
            INSERT INTO ""Transactions"" (""TransactionDate"", ""Amount"", ""PaymentMethod"", ""TransactionType"", 
                ""CategoryId"", ""AccountId"", ""IsActive"", ""BankId"", ""CardId"", ""CreatedAt"")
            VALUES (@TransactionDate, @Amount, @PaymentMethod, @TransactionType, 
                @CategoryId, @AccountId, @IsActive, @BankId, @CardId, @CreatedAt)
            RETURNING ""Id"";
        ";

        var parameters = new DynamicParameters();
        parameters.Add("@TransactionDate", transaction.TransactionDate);
        parameters.Add("@Amount", transaction.Amount);
        parameters.Add("@PaymentMethod", (int)transaction.PaymentMethod);
        parameters.Add("@TransactionType", (int)transaction.TransactionType);
        parameters.Add("@CategoryId", transaction.CategoryId);
        parameters.Add("@AccountId", transaction.AccountId);
        parameters.Add("@IsActive", transaction.IsActive);
        parameters.Add("@BankId", transaction is GeneralTransaction gt ? gt.BankId : (long?)null);
        parameters.Add("@CardId", transaction is CardTransaction ct ? ct.CardId : (long?)null);
        parameters.Add("@CreatedAt", transaction.CreatedAt);

        return (sql, parameters);
    }
}
