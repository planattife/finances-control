namespace FinancesControl.API.TransactionCategories.DTOs;

public record TransactionCategoryDetailResponse(
    int Id,
    string Name,
    string Status,
    DateTime CreatedAt,
    DateTime? LastUpdate
);