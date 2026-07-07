using System;

namespace FinancesControl.API.Transactions.DTOs;

public class TransactionListItemResponse
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public string TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public TransactionCategoryResponse Category { get; set; } = new();
    public string Status { get; set; }
}
