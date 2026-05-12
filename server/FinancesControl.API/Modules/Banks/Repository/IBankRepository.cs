using FinancesControl.API.Models;

namespace FinancesControl.API.Banks.Repository;

public interface IBankRepository
{
    long Add(Bank bank);
    Bank? GetById(long bankId, string userId);
    List<Bank> GetAll(string userId);
    bool Update(Bank bank);
}