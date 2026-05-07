using FinancesControl.API.Models;

namespace FinancesControl.API.TransactionCategories.Repository;

public interface ITransactionCategoryRepository
{
    long Add(TransactionCategory category);
    TransactionCategory? GetById(long categoryId, string? userId);
    List<TransactionCategory> GetAll(string? userId);
    bool Update(TransactionCategory category);
} 