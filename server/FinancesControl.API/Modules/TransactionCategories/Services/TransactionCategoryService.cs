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

    public async Task<long> CreateCategory(CreateTransactionCategoryRequest request, string? userId)
    {
        var category = new TransactionCategory(request.Name, 0, userId);
        return await _repository.Add(category);
    }

    public async Task<TransactionCategoryDetailResponse?> GetById(long categoryId, string? userId)
    {
        var cat = await _repository.GetById(categoryId, userId);
        if (cat == null) return null;
        return new TransactionCategoryDetailResponse(
            cat.Id,
            cat.Name,
            cat.IsActive ? "Active" : "Inactive",
            cat.CreatedAt,
            cat.UpdatedAt
        );
    }

    public async Task<List<TransactionCategoryListItemResponse>> GetAll(string? userId)
    {
        var categories = await _repository.GetAll(userId);
        return categories
            .Select(c => new TransactionCategoryListItemResponse(c.Id, c.Name))
            .ToList();
    }

    public async Task<bool> Rename(long categoryId, RenameTransactionCategoryRequest request, string? userId)
    {
        var cat = await _repository.GetById(categoryId, userId);
        if (cat == null) return false;
        
        // Prevent renaming default categories
        if (cat.IsDefault) return false;
        
        cat.Rename(request.Name);
        return await _repository.Update(cat);
    }

    public async Task<bool> Activate(long categoryId, string? userId)
    {
        var cat = await _repository.GetById(categoryId, userId);
        if (cat == null) return false;
        
        // Prevent activating default categories (they're already managed)
        if (cat.IsDefault) return false;
        
        cat.Activate();
        return await _repository.Update(cat);
    }

    public async Task<bool> Deactivate(long categoryId, string? userId)
    {
        var cat = await _repository.GetById(categoryId, userId);
        if (cat == null) return false;
        
        // Prevent deactivating default categories (they're always available)
        if (cat.IsDefault) return false;
        
        cat.Deactivate();
        return await _repository.Update(cat);
    }
}