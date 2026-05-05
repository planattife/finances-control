using FinancesControl.API.TransactionCategories.DTOs;
using FinancesControl.API.TransactionCategories.Repository;
using FinancesControl.API.Models;

namespace FinancesControl.API.TransactionCategories.Services;

public class TransactionCategoryService : ITransactionCategoryService
{
    private readonly ITransactionCategoryRepository _repository;

    public TransactionCategoryService(ITransactionCategoryRepository repository)
    {
        _repository = repository;
    }

    public long CreateCategory(CreateTransactionCategoryRequest request)
    {
        var category = new TransactionCategory(request.Name, 0);
        return _repository.Add(category);
    }

    public TransactionCategoryDetailResponse? GetById(long categoryId)
    {
        var cat = _repository.GetById(categoryId);
        if (cat == null) return null;
        return new TransactionCategoryDetailResponse(
            cat.Id,
            cat.Name,
            cat.IsActive ? "Active" : "Inactive",
            cat.CreatedAt,
            cat.UpdatedAt
        );
    }

    public List<TransactionCategoryListItemResponse> GetAll()
    {
        return _repository.GetAll()
            .Select(c => new TransactionCategoryListItemResponse(c.Id, c.Name))
            .ToList();
    }

    public bool Rename(long categoryId, RenameTransactionCategoryRequest request)
    {
        var cat = _repository.GetById(categoryId);
        if (cat == null) return false;
        cat.Rename(request.Name);
        return _repository.Update(cat);
    }

    public bool Activate(long categoryId)
    {
        var cat = _repository.GetById(categoryId);
        if (cat == null) return false;
        cat.Activate();
        return _repository.Update(cat);
    }

    public bool Deactivate(long categoryId)
    {
        var cat = _repository.GetById(categoryId);
        if (cat == null) return false;
        cat.Deactivate();
        return _repository.Update(cat);
    }
}