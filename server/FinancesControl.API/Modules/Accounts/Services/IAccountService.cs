using System;
using FinancesControl.API.Accounts.DTOs;

namespace FinancesControl.API.Accounts.Services;

public interface IAccountService
{
    long CreateAccount(CreateAccountRequest request, string userId);
    AccountDetailResponse? GetById(long accountId, string userId);
    List<AccountListItemResponse> GetAll(string userId);
    bool Update(long accountId, UpdateAccountRequest request, string userId);
    bool Activate(long accountId, string userId);
    bool Deactivate(long accountId, string userId);
}
