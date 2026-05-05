using System;
using FinancesControl.API.Accounts.DTOs;

namespace FinancesControl.API.Accounts.Services;

public interface IAccountService
{
    long CreateAccount(CreateAccountRequest request, string userId);
    AccountDetailResponse? GetById(long accountId);
    List<AccountListItemResponse> GetAll();
    bool Update(long accountId, UpdateAccountRequest request);
    bool Activate(long accountId);
    bool Deactivate(long accountId);
}
