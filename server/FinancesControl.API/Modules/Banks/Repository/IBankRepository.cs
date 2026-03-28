using FinancesControl.API.Models;

namespace FinancesControl.API.Banks.Repository;

public interface IBankRepository
{
    int Add(Bank bank);
    Bank? GetById(int bankId);
    List<Bank> GetAll();
    bool Update(Bank bank);
}