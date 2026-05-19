using System.Security.Claims;
using FinancesControl.API.Banks.DTOs;
using FinancesControl.API.Banks.Repository;
using FinancesControl.API.Banks;

namespace FinancesControl.API.Banks.Services;

public class BankService : IBankService
{
    private readonly IBankRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BankService(IBankRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException("User context is not available");
    }

    public async Task<long> CreateBank(CreateBankRequest request)
    {
        var userId = GetCurrentUserId();
        var bank = new Bank(request.Name, request.AccountType, 0, userId);
        return await _repository.Add(bank);
    }

    public async Task<BankDetailResponse?> GetById(long bankId, string userId)
    {
        var bank = await _repository.GetById(bankId, userId);
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

    public async Task<List<BankListItemResponse>> GetAll(string userId)
    {
        var banks = await _repository.GetAll(userId);
        return banks
            .Select(b => new BankListItemResponse(b.Id, b.Name, b.AccountType))
            .ToList();
    }

    public async Task<bool> Update(long bankId, UpdateBankRequest request, string userId)
    {
        var bank = await _repository.GetById(bankId, userId);
        if (bank == null) return false;
        var updatedBank = new Bank(request.Name, request.AccountType, bank.Id, userId);
        if (bank.IsActive)
            updatedBank.Activate();
        else
            updatedBank.Deactivate();
        return await _repository.Update(updatedBank);
    }

    public async Task<bool> Activate(long bankId, string userId)
    {
        var bank = await _repository.GetById(bankId, userId);
        if (bank == null) return false;
        bank.Activate();
        return await _repository.Update(bank);
    }

    public async Task<bool> Deactivate(long bankId, string userId)
    {
        var bank = await _repository.GetById(bankId, userId);
        if (bank == null) return false;
        bank.Deactivate();
        return await _repository.Update(bank);
    }

    public async Task<bool> ValidateBankForUser(long bankId, string userId)
    {
        var bank = await _repository.GetById(bankId, userId);
        return bank != null && bank.IsActive;
    }
}