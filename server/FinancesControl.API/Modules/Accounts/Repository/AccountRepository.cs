using System;
using System.Collections.Generic;
using System.Linq;

namespace FinancesControl.API.Accounts.Repository;

public class AccountRepository : IAccountRepository
{
    private static readonly List<Account> _accounts = new();

    public int Add(Account account)
    {
        _accounts.Add(account);
        return account.Id;
    }

    public Account? GetById(int accountId)
    {
        return _accounts.FirstOrDefault(a => a.Id == accountId);
    }

    public List<Account> GetAll()
    {
        return _accounts.ToList();
    }

    public bool Update(Account account)
    {
        var idx = _accounts.FindIndex(a => a.Id == account.Id);
        if (idx == -1) return false;
        _accounts[idx] = account;
        return true;
    }
}
