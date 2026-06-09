using System.Security.Claims;
using FinancesControl.API.Accounts.Services;
using FinancesControl.API.Banks.Services;
using FinancesControl.API.Cards.DTOs;
using FinancesControl.API.Cards.Repository;
using FinancesControl.API.Common;
using FinancesControl.API.Enums;
using FinancesControl.API.Models;

namespace FinancesControl.API.Cards.Services;

public class CardService : ICardService
{
    private readonly ICardRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBankService _bankService;
    private readonly IAccountService _accountService;

    public CardService(ICardRepository repository, IHttpContextAccessor httpContextAccessor, IBankService bankService, IAccountService accountService)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
        _bankService = bankService;
        _accountService = accountService;
    }

    private string GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId ?? throw new AuthenticationContextException("User context is not available.");
    }

    public async Task<long> CreateDebitCard(CreateDebitCardRequest request, long accountId, string userId)
    {
        if (!await _accountService.ExistsAsync(accountId, userId))
            throw new ResourceNotFoundException("Account not found.");
        
        if (!await _bankService.ValidateBankForUser(request.BankId, userId))
            throw new BusinessRuleException("Bank is not valid.");
        
        var card = new Card(CardMaskingHelper.MaskCardNumber(request.CardNumber), request.ExpirationDate, request.CardNetwork, request.BankId, accountId, CardType.Debit, 0, request.CardName);
        return await _repository.Add(card);
    }

    public async Task<long> CreateCreditCard(CreateCreditCardRequest request, long accountId, string userId)
    {
        if (!await _accountService.ExistsAsync(accountId, userId))
            throw new ResourceNotFoundException("Account not found.");
        
        if (!await _bankService.ValidateBankForUser(request.BankId, userId))
            throw new BusinessRuleException("Bank is not valid.");
        
        var card = new CreditCard(CardMaskingHelper.MaskCardNumber(request.CardNumber), request.ExpirationDate, request.CardNetwork, request.BankId, accountId, request.ClosingDay, request.DueDateDay, CardType.Credit, 0, request.CardName);
        return await _repository.Add(card);
    }

    public async Task<SingleResponse<CardDetailResponse>> GetById(long cardId, long accountId, string userId)
    {
        var response = new SingleResponse<CardDetailResponse>();
        
        if (!await _accountService.ExistsAsync(accountId, userId))
        {
            response.AddError("Account not found.");
            response.SetHttpStatusCode(404);
            return response;
        }

        var card = await _repository.GetById(cardId, accountId);
        if (card == null)
        {
            response.AddError("Card not found.");
            response.SetHttpStatusCode(404);
            return response;
        }

        response.Data = new CardDetailResponse(
            card.Id,
            card.CardNumber,
            card.CardType,
            card.CardNetwork,
            card.ExpirationDate,
            card.BankId,
            card.CardName ?? string.Empty,
            card.IsActive ? "Active" : "Inactive",
            card.CreatedAt,
            card.UpdatedAt
        );
        response.SetHttpStatusCode(200);
        return response;
    }

    public async Task<SingleResponse<CreditCardDetailResponse>> GetCreditById(long cardId, long accountId, string userId)
    {
        var response = new SingleResponse<CreditCardDetailResponse>();
        
        if (!await _accountService.ExistsAsync(accountId, userId))
        {
            response.AddError("Account not found.");
            response.SetHttpStatusCode(404);
            return response;
        }

        var card = await _repository.GetById(cardId, accountId);
        if (card == null || card is not CreditCard creditCard)
        {
            response.AddError("Credit card not found.");
            response.SetHttpStatusCode(404);
            return response;
        }

        var nextClosing = DateHelper.GetNextOccurrence(creditCard.ClosingDay);
        var nextDue = DateHelper.GetNextOccurrence(creditCard.DueDateDay);

        response.Data = new CreditCardDetailResponse(
            creditCard.Id,
            creditCard.CardNumber,
            creditCard.CardType,
            creditCard.CardNetwork,
            creditCard.ExpirationDate,
            creditCard.BankId,
            creditCard.CardName ?? string.Empty,
            nextClosing,
            nextDue,
            creditCard.IsActive ? "Active" : "Inactive",
            creditCard.CreatedAt,
            creditCard.UpdatedAt
        );
        response.SetHttpStatusCode(200);
        return response;
    }

    public async Task<PaginatedResponse<CardListItemResponse>> GetAll(long accountId, string userId, PaginationParams paginationParams)
    {
        var response = new PaginatedResponse<CardListItemResponse>();
        
        if (!await _accountService.ExistsAsync(accountId, userId))
        {
            response.AddError("Account not found.");
            response.SetHttpStatusCode(404);
            return response;
        }

        var cards = await _repository.GetAll(accountId, paginationParams);
        var totalCount = await _repository.CountByAccountId(accountId);
        var data = cards
            .Select(c => new CardListItemResponse(
                c.Id,
                c.CardNumber,
                c.CardType,
                c.CardNetwork,
                c.BankId,
                c.CardName
            ))
            .ToList();
        
        response.Data = data;
        var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);
        var hasNextPage = paginationParams.PageNumber < totalPages;
        var hasPrevPage = paginationParams.PageNumber > 1;

        response.SetPagination(totalCount, totalPages, paginationParams.PageNumber, hasNextPage, hasPrevPage);
        response.SetHttpStatusCode(200);
        return response;
    }

    public async Task<bool> UpdateCardName(long cardId, long accountId, string userId, string cardName)
    {
        if (!await _accountService.ExistsAsync(accountId, userId))
            return false;

        var card = await _repository.GetById(cardId, accountId);
        if (card == null) return false;
        
        card.UpdateCardName(cardName);

        return await _repository.Update(card, accountId);
    }

    public async Task<bool> UpdateCreditCardDates(long cardId, long accountId, string userId, UpdateCreditCardRequest request)
    {
        if (!await _accountService.ExistsAsync(accountId, userId))
            return false;

        var card = await _repository.GetById(cardId, accountId);
        if (card == null || card is not CreditCard creditCard) return false;
        creditCard.UpdateClosingDay(request.ClosingDay);
        creditCard.UpdateDueDateDay(request.DueDateDay);
        
        return await _repository.Update(creditCard, accountId);
    }

    public async Task<bool> Activate(long cardId, long accountId, string userId)
    {
        if (!await _accountService.ExistsAsync(accountId, userId))
            return false;

        var card = await _repository.GetById(cardId, accountId);
        if (card == null) return false;
        card.SetIsActive(true);
        return await _repository.Update(card, accountId);
    }

    public async Task<bool> Deactivate(long cardId, long accountId, string userId)
    {
        if (!await _accountService.ExistsAsync(accountId, userId))
            return false;

        var card = await _repository.GetById(cardId, accountId);
        if (card == null) return false;
        card.SetIsActive(false);
        return await _repository.Update(card, accountId);
    }

    public async Task<bool> ExistsAsync(long cardId, long accountId)
    {
        return await _repository.ExistsAsync(cardId, accountId);
    }
}
