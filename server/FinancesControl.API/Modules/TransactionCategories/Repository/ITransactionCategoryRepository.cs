using FinancesControl.API.Models;

namespace FinancesControl.API.TransactionCategories.Repository;

public interface ITransactionCategoryRepository
{
    long Add(TransactionCategory category);
    TransactionCategory? GetById(long categoryId);
    List<TransactionCategory> GetAll();
    bool Update(TransactionCategory category);
} 