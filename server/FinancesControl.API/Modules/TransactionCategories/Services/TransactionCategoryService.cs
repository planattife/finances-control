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

    public long CreateCategory(CreateTransactionCategoryRequest request, string? userId)
    {
        var category = new TransactionCategory(request.Name, 0, userId);
        return _repository.Add(category);
    }

    public TransactionCategoryDetailResponse? GetById(long categoryId, string? userId)
    {
        var cat = _repository.GetById(categoryId, userId);
        if (cat == null) return null;
        return new TransactionCategoryDetailResponse(
            cat.Id,
            cat.Name,
            cat.IsActive ? "Active" : "Inactive",
            cat.CreatedAt,
            cat.UpdatedAt
        );
    }

    public List<TransactionCategoryListItemResponse> GetAll(string? userId)
    {
        return _repository.GetAll(userId)
            .Select(c => new TransactionCategoryListItemResponse(c.Id, c.Name))
            .ToList();
    }

    public bool Rename(long categoryId, RenameTransactionCategoryRequest request, string? userId)
    {
        var cat = _repository.GetById(categoryId, userId);
        if (cat == null) return false;
        
        // Prevent renaming default categories
        if (cat.IsDefault) return false;
        
        cat.Rename(request.Name);
        return _repository.Update(cat);
    }

    public bool Activate(long categoryId, string? userId)
    {
        var cat = _repository.GetById(categoryId, userId);
        if (cat == null) return false;
        
        // Prevent activating default categories (they're already managed)
        if (cat.IsDefault) return false;
        
        cat.Activate();
        return _repository.Update(cat);
    }

    public bool Deactivate(long categoryId, string? userId)
    {
        var cat = _repository.GetById(categoryId, userId);
        if (cat == null) return false;
        
        // Prevent deactivating default categories (they're always available)
        if (cat.IsDefault) return false;
        
        cat.Deactivate();
        return _repository.Update(cat);
    }
}