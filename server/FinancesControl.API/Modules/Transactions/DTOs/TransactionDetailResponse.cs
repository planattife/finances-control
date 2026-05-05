namespace FinancesControl.API.Transactions.DTOs;

public class TransactionDetailResponse
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public string TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public long CategoryId { get; set; }
    public long? CardId { get; set; }
    public long? BankId { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}