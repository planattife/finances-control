using FinancesControl.API.TransactionCategories.DTOs;

namespace FinancesControl.API.TransactionCategories.Services;

public interface ITransactionCategoryService
{
    long CreateCategory(CreateTransactionCategoryRequest request, string? userId);
    TransactionCategoryDetailResponse? GetById(long categoryId, string? userId);
    List<TransactionCategoryListItemResponse> GetAll(string? userId);
    bool Rename(long categoryId, RenameTransactionCategoryRequest request, string? userId);
    bool Activate(long categoryId, string? userId);
    bool Deactivate(long categoryId, string? userId);
}