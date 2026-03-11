namespace FinancesControl.API.Transactions.DTOs;

public class TransactionDetailResponse
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public string TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public int CategoryId { get; set; }
    public int? CardId { get; set; }
    public int? BankId { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}