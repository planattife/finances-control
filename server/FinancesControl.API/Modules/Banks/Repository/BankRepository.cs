using System.Collections.Generic;
using System.Linq;
using FinancesControl.API.Models;

namespace FinancesControl.API.Banks.Repository;

public class BankRepository : IBankRepository
{
    private static readonly List<Bank> _banks = new();

    public int Add(Bank bank)
    {
        _banks.Add(bank);
        return bank.Id;
    }

    public Bank? GetById(int bankId)
    {
        return _banks.FirstOrDefault(b => b.Id == bankId);
    }

    public List<Bank> GetAll()
    {
        return _banks.ToList();
    }

    public bool Update(Bank bank)
    {
        var idx = _banks.FindIndex(b => b.Id == bank.Id);
        if (idx == -1) return false;
        _banks[idx] = bank;
        return true;
    }
}