using System.Collections.Generic;
using System.Linq;
using FinancesControl.API.Models;

namespace FinancesControl.API.TransactionCategories.Repository;

public class TransactionCategoryRepository : ITransactionCategoryRepository
{
    private static readonly List<TransactionCategory> _categories = new();

    public int Add(TransactionCategory category)
    {
        _categories.Add(category);
        return category.Id;
    }

    public TransactionCategory? GetById(int categoryId)
    {
        return _categories.FirstOrDefault(c => c.Id == categoryId);
    }

    public List<TransactionCategory> GetAll()
    {
        return _categories.ToList();
    }

    public bool Update(TransactionCategory category)
    {
        var idx = _categories.FindIndex(c => c.Id == category.Id);
        if (idx == -1) return false;
        _categories[idx] = category;
        return true;
    }
}