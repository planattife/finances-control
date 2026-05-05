using System.Security.Claims;
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

    public CardService(ICardRepository repository, IHttpContextAccessor httpContextAccessor, IBankService bankService)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
        _bankService = bankService;
    }

    private string GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId ?? throw new InvalidOperationException("User context not available");
    }

    public long CreateDebitCard(CreateDebitCardRequest request)
    {
        var userId = GetCurrentUserId();
        if (!_bankService.ValidateBankForUser(request.BankId, userId))
            throw new InvalidOperationException("Bank is not valid.");
        
        var card = new Card(CardMaskingHelper.MaskCardNumber(request.CardNumber), request.ExpirationDate, request.CardNetwork, request.BankId, CardType.Debit, 0, request.CardName);
        return _repository.Add(card);
    }

    public long CreateCreditCard(CreateCreditCardRequest request)
    {
        var userId = GetCurrentUserId();
        if (!_bankService.ValidateBankForUser(request.BankId, userId))
            throw new InvalidOperationException("Bank is not valid.");
        
        var card = new CreditCard(CardMaskingHelper.MaskCardNumber(request.CardNumber), request.ExpirationDate, request.CardNetwork, request.BankId, request.ClosingDay, request.DueDateDay, CardType.Credit, 0, request.CardName);
        return _repository.Add(card);
    }

    public CardDetailResponse? GetById(long cardId)
    {
        var card = _repository.GetById(cardId);
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

    public CreditCardDetailResponse? GetCreditById(long cardId)
    {
        var card = _repository.GetById(cardId);
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

    public List<CardListItemResponse> GetAll()
    {
        return _repository.GetAll()
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

    public bool UpdateCardName(long cardId, string cardName)
    {
        var card = _repository.GetById(cardId);
        if (card == null) return false;
        
        card.UpdateCardName(cardName);

        return _repository.Update(card);
    }

    public bool UpdateCreditCardDates(long cardId, UpdateCreditCardRequest request)
    {
        var card = _repository.GetById(cardId);
        if (card == null || card is not CreditCard creditCard) return false;
        creditCard.UpdateClosingDay(request.ClosingDay);
        creditCard.UpdateDueDateDay(request.DueDateDay);
        
        return _repository.Update(creditCard);
    }

    public bool Activate(long cardId)
    {
        var card = _repository.GetById(cardId);
        if (card == null) return false;
        card.SetIsActive(true);
        return _repository.Update(card);
    }

    public bool Deactivate(long cardId)
    {
        var card = _repository.GetById(cardId);
        if (card == null) return false;
        card.SetIsActive(false);
        return _repository.Update(card);
    }
}