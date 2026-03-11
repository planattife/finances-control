using System;
using FinancesControl.API.Enums;

namespace FinancesControl.API.Transactions.DTOs;

public class TransactionListItemResponse
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public string TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public int CategoryId { get; set; }
    public string Status { get; set; }
}