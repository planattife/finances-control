namespace FinancesControl.API.Transactions.DTOs;

public class UpdateTransactionRequest
{
    public decimal? NewAmount { get; set; }
    public long? NewCategoryId { get; set; }
    public string? NewDescription { get; set; }
    public bool HasDescriptionUpdate { get; set; }
}
