using FinancesControl.API.TransactionCategories.DTOs;
using FinancesControl.API.TransactionCategories.Repository;
using FinancesControl.API.Common;
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

    public async Task<SingleResponse<TransactionCategoryDetailResponse>> GetById(long categoryId, string? userId)
    {
        var response = new SingleResponse<TransactionCategoryDetailResponse>();

        var cat = await _repository.GetById(categoryId, userId);
        if (cat == null)
        {
            response.AddError("Category not found.");
            response.SetHttpStatusCode(404);
            return response;
        }

        response.Data = new TransactionCategoryDetailResponse(
            cat.Id,
            cat.Name,
            cat.IsActive ? "Active" : "Inactive",
            cat.CreatedAt,
            cat.UpdatedAt
        );
        response.SetHttpStatusCode(200);
        return response;
    }

    public async Task<PaginatedResponse<TransactionCategoryListItemResponse>> GetAll(string? userId, PaginationParams paginationParams)
    {
        var response = new PaginatedResponse<TransactionCategoryListItemResponse>();

        var categories = await _repository.GetAll(userId, paginationParams);
        var totalCount = await _repository.CountByUserId(userId);
        var data = categories
            .Select(c => new TransactionCategoryListItemResponse(c.Id, c.Name, c.IsDefault))
            .ToList();
        
        response.Data = data;
        var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);
        var hasNextPage = paginationParams.PageNumber < totalPages;
        var hasPrevPage = paginationParams.PageNumber > 1;

        response.SetPagination(totalCount, totalPages, paginationParams.PageNumber, hasNextPage, hasPrevPage);
        response.SetHttpStatusCode(200);
        return response;
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

    public async Task<bool> ExistsAsync(long categoryId)
    {
        return await _repository.ExistsAsync(categoryId);
    }
}
