using FinancesControl.API.Banks.DTOs;
using FinancesControl.API.Common;

namespace FinancesControl.API.Banks.Services;

public interface IBankService
{
    Task<long> CreateBank(CreateBankRequest request);
    Task<SingleResponse<BankDetailResponse>> GetById(long bankId, string userId);
    Task<PaginatedResponse<BankListItemResponse>> GetAll(string userId);
    Task<bool> Update(long bankId, UpdateBankRequest request, string userId);
    Task<bool> Activate(long bankId, string userId);
    Task<bool> Deactivate(long bankId, string userId);
    Task<bool> ValidateBankForUser(long bankId, string userId);
    Task<bool> ExistsAsync(long bankId, string userId);
}