using System;
using FinancesControl.API.Accounts.DTOs;
using FinancesControl.API.Accounts.Repository;
using FinancesControl.API.Common;

namespace FinancesControl.API.Accounts.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;

    public AccountService(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<long> CreateAccount(CreateAccountRequest request, string userId)
    {
        var account = new Account(0, request.Name, userId);
        return await _repository.Add(account);
    }

    public async Task<SingleResponse<AccountDetailResponse>> GetById(long accountId, string userId)
    {
        var response = new SingleResponse<AccountDetailResponse>();

        var account = await _repository.GetById(accountId, userId);

        if (account == null)
        {
            response.Data = null;
            response.AddError("Account not found.");
            response.SetHttpStatusCode(404);
            return response;
        }
        
        var accountDetail = new AccountDetailResponse(
            account.Id,
            account.Name,
            account.IsActive ? StatusEnum.Active.ToString() : StatusEnum.Inactive.ToString(),
            account.CreatedAt,
            account.UpdatedAt
        );
        
        response.Data = accountDetail;
        return response;
    }

    public async Task<PaginatedResponse<AccountListItemResponse>> GetAll(string userId)
    {
        var response = new PaginatedResponse<AccountListItemResponse>();

        var accounts = await _repository.GetAll(userId);

        var data = accounts
            .Select(a => new AccountListItemResponse(a.Id, a.Name))
            .ToList();
        
        response.Data = data;
        response.SetPagination(1, 1, false, false);

        return response;
    }

    public async Task<bool> Update(long accountId, UpdateAccountRequest request, string userId)
    {
        var account = await _repository.GetById(accountId, userId);
        if (account == null) return false;
        account.Rename(request.Name);
        return await _repository.Update(account);
    }

    public async Task<bool> Activate(long accountId, string userId)
    {
        var account = await _repository.GetById(accountId, userId);
        if (account == null) return false;
        account.Activate();
        return await _repository.Update(account);
    }

    public async Task<bool> Deactivate(long accountId, string userId)
    {
        var account = await _repository.GetById(accountId, userId);
        if (account == null) return false;
        account.Deactivate();
        return await _repository.Update(account);
    }
}
