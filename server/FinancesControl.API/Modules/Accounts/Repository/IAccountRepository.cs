using System;
using System.Collections.Generic;

namespace FinancesControl.API.Accounts.Repository;

public interface IAccountRepository
{
    long Add(Account account);
    Account? GetById(long accountId, string userId);
    List<Account> GetAll(string userId);
    bool Update(Account account);
}
