using FinancesControl.API.TransactionCategories.DTOs;

namespace FinancesControl.API.TransactionCategories.Services;

public interface ITransactionCategoryService
{
    long CreateCategory(CreateTransactionCategoryRequest request);
    TransactionCategoryDetailResponse? GetById(long categoryId);
    List<TransactionCategoryListItemResponse> GetAll();
    bool Rename(long categoryId, RenameTransactionCategoryRequest request);
    bool Activate(long categoryId);
    bool Deactivate(long categoryId);
}