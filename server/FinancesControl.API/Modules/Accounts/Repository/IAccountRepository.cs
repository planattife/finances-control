using System;
using System.Collections.Generic;

namespace FinancesControl.API.Accounts.Repository;

public interface IAccountRepository
{
    long Add(Account account);
    Account? GetById(long accountId);
    List<Account> GetAll();
    bool Update(Account account);
}
