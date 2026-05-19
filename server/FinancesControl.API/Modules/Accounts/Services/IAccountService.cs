using System;
using FinancesControl.API.Accounts.DTOs;

namespace FinancesControl.API.Accounts.Services;

public interface IAccountService
{
    Task<long> CreateAccount(CreateAccountRequest request, string userId);
    Task<AccountDetailResponse?> GetById(long accountId, string userId);
    Task<List<AccountListItemResponse>> GetAll(string userId);
    Task<bool> Update(long accountId, UpdateAccountRequest request, string userId);
    Task<bool> Activate(long accountId, string userId);
    Task<bool> Deactivate(long accountId, string userId);
}
