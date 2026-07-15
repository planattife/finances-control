using FinancesControl.API.Models;
using FinancesControl.API.Common;

namespace FinancesControl.API.TransactionCategories.Repository;

public interface ITransactionCategoryRepository
{
    Task<long> Add(TransactionCategory category);
    Task<TransactionCategory?> GetById(long categoryId, string? userId);
    Task<List<TransactionCategory>> GetAll(string? userId, PaginationParams paginationParams);
    Task<int> CountByUserId(string? userId);
    Task<bool> Update(TransactionCategory category);
    Task<bool> ExistsAsync(long categoryId, string? userId);
} 
