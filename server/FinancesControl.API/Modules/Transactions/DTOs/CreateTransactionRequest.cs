using FinancesControl.API.Enums;

namespace FinancesControl.API.Transactions.DTOs;

public class CreateTransactionRequest
{
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public TransactionType TransactionType { get; set; }
    public long CategoryId { get; set; }
    public long AccountId { get; set; }
    public long? CardId { get; set; }
    public long? BankId { get; set; }
    public DateTime TransactionDate { get; set; }
}