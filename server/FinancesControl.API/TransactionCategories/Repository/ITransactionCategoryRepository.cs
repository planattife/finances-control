using FinancesControl.API.Models;

namespace FinancesControl.API.TransactionCategories.Repository;

public interface ITransactionCategoryRepository
{
    int Add(TransactionCategory category);
    TransactionCategory? GetById(int categoryId);
    List<TransactionCategory> GetAll();
    bool Update(TransactionCategory category);
} 