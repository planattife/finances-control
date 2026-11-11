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

    public async Task<SingleResponse<AccountKpiResponse>> GetAccountKpi(long accountId, string userId)
    {
        var response = new SingleResponse<AccountKpiResponse>();

        var accountKpi = await _repository.GetAccountKpi(accountId, userId);

        if (accountKpi == null)
        {
            response.Data = null;
            response.AddError("Account not found.");
            response.SetHttpStatusCode(404);
            return response;
        }

        response.Data = accountKpi;
        return response;
    }

    public async Task<PaginatedResponse<AccountListItemResponse>> GetAll(string userId, PaginationParams paginationParams)
    {
        var response = new PaginatedResponse<AccountListItemResponse>();

        var accounts = await _repository.GetAll(userId, paginationParams);
        var totalCount = await _repository.CountByUserId(userId);

        var data = accounts
            .Select(a => new AccountListItemResponse(a.Id, a.Name, a.IsActive))
            .ToList();
        
        response.Data = data;
        var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);
        var hasNextPage = paginationParams.PageNumber < totalPages;
        var hasPrevPage = paginationParams.PageNumber > 1;

        response.SetPagination(totalCount, totalPages, paginationParams.PageNumber, hasNextPage, hasPrevPage);
        response.SetHttpStatusCode(200);

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

    public async Task<bool> ExistsAsync(long accountId, string userId)
    {
        return await _repository.ExistsAsync(accountId, userId);
    }
}
