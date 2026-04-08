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

    public int CreateAccount(CreateAccountRequest request, string userId)
    {
        var account = new Account(0, request.Name, userId);
        return _repository.Add(account);
    }

    public AccountDetailResponse? GetById(int accountId)
    {
        var account = _repository.GetById(accountId);
        if (account == null) return null;
        return new AccountDetailResponse(
            account.Id,
            account.Name,
            account.IsActive ? StatusEnum.Active.ToString() : StatusEnum.Inactive.ToString(),
            account.CreatedAt,
            account.UpdatedAt
        );
    }

    public List<AccountListItemResponse> GetAll()
    {
        return _repository.GetAll()
            .Select(a => new AccountListItemResponse(a.Id, a.Name))
            .ToList();
    }

    public bool Update(int accountId, UpdateAccountRequest request)
    {
        var account = _repository.GetById(accountId);
        if (account == null) return false;
        account.Rename(request.Name);
        return _repository.Update(account);
    }

    public bool Activate(int accountId)
    {
        var account = _repository.GetById(accountId);
        if (account == null) return false;
        account.Activate();
        return _repository.Update(account);
    }

    public bool Deactivate(int accountId)
    {
        var account = _repository.GetById(accountId);
        if (account == null) return false;
        account.Deactivate();
        return _repository.Update(account);
    }
}
