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

    public long CreateAccount(CreateAccountRequest request, string userId)
    {
        var account = new Account(0, request.Name, userId);
        return _repository.Add(account);
    }

    public AccountDetailResponse? GetById(long accountId, string userId)
    {
        var account = _repository.GetById(accountId, userId);
        if (account == null) return null;
        return new AccountDetailResponse(
            account.Id,
            account.Name,
            account.IsActive ? StatusEnum.Active.ToString() : StatusEnum.Inactive.ToString(),
            account.CreatedAt,
            account.UpdatedAt
        );
    }

    public List<AccountListItemResponse> GetAll(string userId)
    {
        return _repository.GetAll(userId)
            .Select(a => new AccountListItemResponse(a.Id, a.Name))
            .ToList();
    }

    public bool Update(long accountId, UpdateAccountRequest request, string userId)
    {
        var account = _repository.GetById(accountId, userId);
        if (account == null) return false;
        account.Rename(request.Name);
        return _repository.Update(account);
    }

    public bool Activate(long accountId, string userId)
    {
        var account = _repository.GetById(accountId, userId);
        if (account == null) return false;
        account.Activate();
        return _repository.Update(account);
    }

    public bool Deactivate(long accountId, string userId)
    {
        var account = _repository.GetById(accountId, userId);
        if (account == null) return false;
        account.Deactivate();
        return _repository.Update(account);
    }
}
