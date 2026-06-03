using System.Security.Claims;
using FinancesControl.API.Banks.DTOs;
using FinancesControl.API.Banks.Repository;
using FinancesControl.API.Common;
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

    public async Task<SingleResponse<BankDetailResponse>> GetById(long bankId, string userId)
    {
        var response = new SingleResponse<BankDetailResponse>();

        var bank = await _repository.GetById(bankId, userId);
        if (bank == null)
        {
            response.AddError("Bank not found.");
            response.SetHttpStatusCode(404);
            return response;
        }

        response.Data = new BankDetailResponse(
            bank.Id,
            bank.Name,
            bank.AccountType,
            bank.IsActive ? "Active" : "Inactive",
            bank.CreatedAt,
            bank.UpdatedAt
        );
        response.SetHttpStatusCode(200);
        return response;
    }

    public async Task<PaginatedResponse<BankListItemResponse>> GetAll(string userId, PaginationParams paginationParams)
    {
        var response = new PaginatedResponse<BankListItemResponse>();

        var banks = await _repository.GetAll(userId, paginationParams);
        var totalCount = await _repository.CountByUserId(userId);
        var data = banks
            .Select(b => new BankListItemResponse(b.Id, b.Name, b.AccountType))
            .ToList();
        
        response.Data = data;
        var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);
        var hasNextPage = paginationParams.PageNumber < totalPages;
        var hasPrevPage = paginationParams.PageNumber > 1;

        response.SetPagination(totalCount, totalPages, paginationParams.PageNumber, hasNextPage, hasPrevPage);
        response.SetHttpStatusCode(200);
        return response;
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

    public async Task<bool> ExistsAsync(long bankId, string userId)
    {
        return await _repository.ExistsAsync(bankId, userId);
    }
}
