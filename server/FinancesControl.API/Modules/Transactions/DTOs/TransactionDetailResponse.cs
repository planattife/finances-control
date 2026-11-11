namespace FinancesControl.API.Transactions.DTOs;

public class TransactionDetailResponse
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public TransactionCategoryResponse Category { get; set; } = new();
    public TransactionCardResponse? Card { get; set; }
    public TransactionBankResponse? Bank { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
