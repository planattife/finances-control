using FinancesControl.API.TransactionCategories.DTOs;

namespace FinancesControl.API.TransactionCategories.Services;

public interface ITransactionCategoryService
{
    Task<long> CreateCategory(CreateTransactionCategoryRequest request, string? userId);
    Task<TransactionCategoryDetailResponse?> GetById(long categoryId, string? userId);
    Task<List<TransactionCategoryListItemResponse>> GetAll(string? userId);
    Task<bool> Rename(long categoryId, RenameTransactionCategoryRequest request, string? userId);
    Task<bool> Activate(long categoryId, string? userId);
    Task<bool> Deactivate(long categoryId, string? userId);
}