using FinancesControl.API.Models;

namespace FinancesControl.API.TransactionCategories.Repository;

public interface ITransactionCategoryRepository
{
    Task<long> Add(TransactionCategory category);
    Task<TransactionCategory?> GetById(long categoryId, string? userId);
    Task<List<TransactionCategory>> GetAll(string? userId);
    Task<bool> Update(TransactionCategory category);
    Task<bool> ExistsAsync(long categoryId);
} 