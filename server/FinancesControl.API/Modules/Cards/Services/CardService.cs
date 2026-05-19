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
        return userId ?? throw new InvalidOperationException("User context not available");
    }

    public async Task<long> CreateDebitCard(CreateDebitCardRequest request, long accountId, string userId)
    {
        if (await _accountService.GetById(accountId, userId) == null)
            throw new InvalidOperationException("Account not found.");
        
        if (!await _bankService.ValidateBankForUser(request.BankId, userId))
            throw new InvalidOperationException("Bank is not valid.");
        
        var card = new Card(CardMaskingHelper.MaskCardNumber(request.CardNumber), request.ExpirationDate, request.CardNetwork, request.BankId, accountId, CardType.Debit, 0, request.CardName);
        return await _repository.Add(card);
    }

    public async Task<long> CreateCreditCard(CreateCreditCardRequest request, long accountId, string userId)
    {
        if (await _accountService.GetById(accountId, userId) == null)
            throw new InvalidOperationException("Account not found.");
        
        if (!await _bankService.ValidateBankForUser(request.BankId, userId))
            throw new InvalidOperationException("Bank is not valid.");
        
        var card = new CreditCard(CardMaskingHelper.MaskCardNumber(request.CardNumber), request.ExpirationDate, request.CardNetwork, request.BankId, accountId, request.ClosingDay, request.DueDateDay, CardType.Credit, 0, request.CardName);
        return await _repository.Add(card);
    }

    public async Task<CardDetailResponse?> GetById(long cardId, long accountId, string userId)
    {
        if (await _accountService.GetById(accountId, userId) == null)
            return null;

        var card = await _repository.GetById(cardId, accountId);
        if (card == null) return null;

        return new CardDetailResponse(
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
    }

    public async Task<CreditCardDetailResponse?> GetCreditById(long cardId, long accountId, string userId)
    {
        if (await _accountService.GetById(accountId, userId) == null)
            return null;

        var card = await _repository.GetById(cardId, accountId);
        if (card == null || card is not CreditCard creditCard) return null;

        var nextClosing = DateHelper.GetNextOccurrence(creditCard.ClosingDay);
        var nextDue = DateHelper.GetNextOccurrence(creditCard.DueDateDay);

        return new CreditCardDetailResponse(
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
    }

    public async Task<List<CardListItemResponse>> GetAll(long accountId, string userId)
    {
        if (await _accountService.GetById(accountId, userId) == null)
            return new List<CardListItemResponse>();

        var cards = await _repository.GetAll(accountId);
        return cards
            .Select(c => new CardListItemResponse(
                c.Id,
                c.CardNumber,
                c.CardType,
                c.CardNetwork,
                c.BankId,
                c.CardName
            ))
            .ToList();
    }

    public async Task<bool> UpdateCardName(long cardId, long accountId, string userId, string cardName)
    {
        if (await _accountService.GetById(accountId, userId) == null)
            return false;

        var card = await _repository.GetById(cardId, accountId);
        if (card == null) return false;
        
        card.UpdateCardName(cardName);

        return await _repository.Update(card, accountId);
    }

    public async Task<bool> UpdateCreditCardDates(long cardId, long accountId, string userId, UpdateCreditCardRequest request)
    {
        if (await _accountService.GetById(accountId, userId) == null)
            return false;

        var card = await _repository.GetById(cardId, accountId);
        if (card == null || card is not CreditCard creditCard) return false;
        creditCard.UpdateClosingDay(request.ClosingDay);
        creditCard.UpdateDueDateDay(request.DueDateDay);
        
        return await _repository.Update(creditCard, accountId);
    }

    public async Task<bool> Activate(long cardId, long accountId, string userId)
    {
        if (await _accountService.GetById(accountId, userId) == null)
            return false;

        var card = await _repository.GetById(cardId, accountId);
        if (card == null) return false;
        card.SetIsActive(true);
        return await _repository.Update(card, accountId);
    }

    public async Task<bool> Deactivate(long cardId, long accountId, string userId)
    {
        if (await _accountService.GetById(accountId, userId) == null)
            return false;

        var card = await _repository.GetById(cardId, accountId);
        if (card == null) return false;
        card.SetIsActive(false);
        return await _repository.Update(card, accountId);
    }
}