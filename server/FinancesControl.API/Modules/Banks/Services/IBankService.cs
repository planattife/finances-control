using FinancesControl.API.Banks.DTOs;

namespace FinancesControl.API.Banks.Services;

public interface IBankService
{
    Task<long> CreateBank(CreateBankRequest request);
    Task<BankDetailResponse?> GetById(long bankId, string userId);
    Task<List<BankListItemResponse>> GetAll(string userId);
    Task<bool> Update(long bankId, UpdateBankRequest request, string userId);
    Task<bool> Activate(long bankId, string userId);
    Task<bool> Deactivate(long bankId, string userId);
    Task<bool> ValidateBankForUser(long bankId, string userId);
}