namespace FinancesControl.API.Transactions.DTOs;

public class TransactionCategoryResponse
{
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
