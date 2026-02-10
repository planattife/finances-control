using FinancesControl.API.Banks.DTOs;

namespace FinancesControl.API.Banks.Services;

public interface IBankService
{
    int CreateBank(CreateBankRequest request);
    BankDetailResponse? GetById(int bankId);
    List<BankListItemResponse> GetAll();
    bool Update(int bankId, UpdateBankRequest request);
    bool Activate(int bankId);
    bool Deactivate(int bankId);
}