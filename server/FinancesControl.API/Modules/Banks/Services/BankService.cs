using FinancesControl.API.Banks.DTOs;
using FinancesControl.API.Banks.Repository;
using FinancesControl.API.Banks;

namespace FinancesControl.API.Banks.Services;

public class BankService : IBankService
{
    private readonly IBankRepository _repository;
    private readonly string _userId = "mock-user-id"; // This should be replaced with actual user context in a real application
    public BankService(IBankRepository repository)
    {
        _repository = repository;
    }

    public int CreateBank(CreateBankRequest request)
    {
        var allBanks = _repository.GetAll();
        int newId = allBanks.Count == 0 ? 1 : allBanks.Max(b => b.Id) + 1;
        var bank = new Bank(request.Name, request.AccountType, newId, _userId);
        return _repository.Add(bank);
    }

    public BankDetailResponse? GetById(int bankId)
    {
        var bank = _repository.GetById(bankId);
        if (bank == null) return null;
        return new BankDetailResponse(
            bank.Id,
            bank.Name,
            bank.AccountType,
            bank.IsActive ? "Active" : "Inactive",
            bank.CreatedAt,
            bank.UpdatedAt
        );
    }

    public List<BankListItemResponse> GetAll()
    {
        return _repository.GetAll()
            .Select(b => new BankListItemResponse(b.Id, b.Name, b.AccountType))
            .ToList();
    }

    public bool Update(int bankId, UpdateBankRequest request)
    {
        var bank = _repository.GetById(bankId);
        if (bank == null) return false;
        bank = new Bank(request.Name, request.AccountType, bank.Id, _userId);
        bank.Activate();
        return _repository.Update(bank);
    }

    public bool Activate(int bankId)
    {
        var bank = _repository.GetById(bankId);
        if (bank == null) return false;
        bank.Activate();
        return _repository.Update(bank);
    }

    public bool Deactivate(int bankId)
    {
        var bank = _repository.GetById(bankId);
        if (bank == null) return false;
        bank.Deactivate();
        return _repository.Update(bank);
    }
}