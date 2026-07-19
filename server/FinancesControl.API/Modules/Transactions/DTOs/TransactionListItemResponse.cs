using System;

namespace FinancesControl.API.Transactions.DTOs;

public class TransactionListItemResponse
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public TransactionCategoryResponse Category { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}
