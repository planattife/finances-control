using System;
using FinancesControl.API.Accounts.DTOs;
using FinancesControl.API.Common;

namespace FinancesControl.API.Accounts.Services;

public interface IAccountService
{
    Task<long> CreateAccount(CreateAccountRequest request, string userId);
    Task<SingleResponse<AccountDetailResponse>> GetById(long accountId, string userId);
    Task<PaginatedResponse<AccountListItemResponse>> GetAll(string userId, PaginationParams paginationParams);
    Task<bool> Update(long accountId, UpdateAccountRequest request, string userId);
    Task<bool> Activate(long accountId, string userId);
    Task<bool> Deactivate(long accountId, string userId);
    Task<bool> ExistsAsync(long accountId, string userId);
}
