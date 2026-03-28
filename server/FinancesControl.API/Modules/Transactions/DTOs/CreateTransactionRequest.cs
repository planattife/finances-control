using FinancesControl.API.Enums;

namespace FinancesControl.API.Transactions.DTOs;

public class CreateTransactionRequest
{
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public TransactionType TransactionType { get; set; }
    public int CategoryId { get; set; }
    public int? CardId { get; set; }
    public int? BankId { get; set; }
    public DateTime TransactionDate { get; set; }}