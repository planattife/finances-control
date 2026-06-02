using System;
using System.Collections.Generic;

namespace FinancesControl.API.Accounts.Repository;

public interface IAccountRepository
{
    Task<long> Add(Account account);
    Task<Account?> GetById(long accountId, string userId);
    Task<List<Account>> GetAll(string userId);
    Task<bool> Update(Account account);
    Task<bool> ExistsAsync(long accountId, string userId);
}
