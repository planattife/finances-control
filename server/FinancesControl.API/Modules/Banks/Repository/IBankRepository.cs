using FinancesControl.API.Models;

namespace FinancesControl.API.Banks.Repository;

public interface IBankRepository
{
    long Add(Bank bank);
    Bank? GetById(long bankId);
    List<Bank> GetAll();
    bool Update(Bank bank);
}