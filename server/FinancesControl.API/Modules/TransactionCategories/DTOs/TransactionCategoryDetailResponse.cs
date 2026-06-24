namespace FinancesControl.API.TransactionCategories.DTOs;

public record TransactionCategoryDetailResponse(
    long Id,
    string Name,
    string Status,
    bool IsDefault,
    DateTime CreatedAt,
    DateTime? LastUpdate
);
