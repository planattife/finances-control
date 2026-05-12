using FinancesControl.API.Banks.DTOs;

namespace FinancesControl.API.Banks.Services;

public interface IBankService
{
    long CreateBank(CreateBankRequest request);
    BankDetailResponse? GetById(long bankId, string userId);
    List<BankListItemResponse> GetAll(string userId);
    bool Update(long bankId, UpdateBankRequest request, string userId);
    bool Activate(long bankId, string userId);
    bool Deactivate(long bankId, string userId);
    bool ValidateBankForUser(long bankId, string userId);
}