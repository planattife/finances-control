using System;
using System.Collections.Generic;

namespace FinancesControl.API.Accounts.Repository;

public interface IAccountRepository
{
    int Add(Account account);
    Account? GetById(int accountId);
    List<Account> GetAll();
    bool Update(Account account);
}
