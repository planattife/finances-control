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

    public int CreateBank(CreateBankRequest request)
    {
        var userId = GetCurrentUserId();
        var bank = new Bank(request.Name, request.AccountType, 0, userId);
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
        var userId = GetCurrentUserId();
        bank = new Bank(request.Name, request.AccountType, bank.Id, userId);
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

    public bool ValidateBankForUser(int bankId, string userId)
    {
        var bank = _repository.GetById(bankId);
        return bank != null && bank.UserId == userId && bank.IsActive;
    }
}