using FinancesControl.API.TransactionCategories.DTOs;

namespace FinancesControl.API.TransactionCategories.Services;

public interface ITransactionCategoryService
{
    int CreateCategory(CreateTransactionCategoryRequest request);
    TransactionCategoryDetailResponse? GetById(int categoryId);
    List<TransactionCategoryListItemResponse> GetAll();
    bool Rename(int categoryId, RenameTransactionCategoryRequest request);
    bool Activate(int categoryId);
    bool Deactivate(int categoryId);
}