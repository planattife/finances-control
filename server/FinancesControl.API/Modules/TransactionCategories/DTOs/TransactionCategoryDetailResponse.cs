namespace FinancesControl.API.TransactionCategories.DTOs;

public record TransactionCategoryDetailResponse(
    long Id,
    string Name,
    string Status,
    DateTime CreatedAt,
    DateTime? LastUpdate
);