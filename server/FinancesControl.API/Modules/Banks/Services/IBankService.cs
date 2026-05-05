using FinancesControl.API.Banks.DTOs;

namespace FinancesControl.API.Banks.Services;

public interface IBankService
{
    long CreateBank(CreateBankRequest request);
    BankDetailResponse? GetById(long bankId);
    List<BankListItemResponse> GetAll();
    bool Update(long bankId, UpdateBankRequest request);
    bool Activate(long bankId);
    bool Deactivate(long bankId);
    bool ValidateBankForUser(long bankId, string userId);
}