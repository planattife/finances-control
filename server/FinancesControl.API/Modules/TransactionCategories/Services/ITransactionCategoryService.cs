using FinancesControl.API.TransactionCategories.DTOs;
using FinancesControl.API.Common;

namespace FinancesControl.API.TransactionCategories.Services;

public interface ITransactionCategoryService
{
    Task<long> CreateCategory(CreateTransactionCategoryRequest request, string? userId);
    Task<SingleResponse<TransactionCategoryDetailResponse>> GetById(long categoryId, string? userId);
    Task<PaginatedResponse<TransactionCategoryListItemResponse>> GetAll(string? userId, PaginationParams paginationParams);
    Task<bool> Rename(long categoryId, RenameTransactionCategoryRequest request, string? userId);
    Task<bool> Activate(long categoryId, string? userId);
    Task<bool> Deactivate(long categoryId, string? userId);
    Task<bool> ExistsAsync(long categoryId, string? userId);
}
