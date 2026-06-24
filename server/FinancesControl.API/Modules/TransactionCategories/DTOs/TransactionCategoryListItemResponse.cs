namespace FinancesControl.API.TransactionCategories.DTOs;

public record TransactionCategoryListItemResponse(
    long Id,
    string Name,
    bool IsDefault
);
