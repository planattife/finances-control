using System;
using FinancesControl.API.Accounts.DTOs;

namespace FinancesControl.API.Accounts.Services;

public interface IAccountService
{
    int CreateAccount(CreateAccountRequest request, string userId);
    AccountDetailResponse? GetById(int accountId);
    List<AccountListItemResponse> GetAll();
    bool Update(int accountId, UpdateAccountRequest request);
    bool Activate(int accountId);
    bool Deactivate(int accountId);
}
