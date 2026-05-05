using System;
using FinancesControl.API.Enums;

namespace FinancesControl.API.Transactions.DTOs;

public class TransactionListItemResponse
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public string TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public long CategoryId { get; set; }
    public string Status { get; set; }
}