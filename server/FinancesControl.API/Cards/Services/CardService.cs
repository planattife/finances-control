using FinancesControl.API.Cards.DTOs;
using FinancesControl.API.Cards.Repository;
using FinancesControl.API.Common;
using FinancesControl.API.Enums;
using FinancesControl.API.Models;

namespace FinancesControl.API.Cards.Services;

public class CardService : ICardService
{
    private readonly ICardRepository _repository;

    public CardService(ICardRepository repository)
    {
        _repository = repository;
    }

    public int CreateDebitCard(CreateDebitCardRequest request)
    {
        var allCards = _repository.GetAll();
        int newId = allCards.Count == 0 ? 1 : allCards.Max(c => c.Id) + 1;
        var card = new Card(request.CardNumber, request.ExpirationDate, request.CardNetwork, request.BankId, CardType.Debit, newId, request.CardName);
        return _repository.Add(card);
    }

    public int CreateCreditCard(CreateCreditCardRequest request)
    {
        var allCards = _repository.GetAll();
        int newId = allCards.Count == 0 ? 1 : allCards.Max(c => c.Id) + 1;
        var card = new CreditCard(request.CardNumber, request.ExpirationDate, request.CardNetwork, request.BankId, request.ClosingDay, request.DueDateDay, CardType.Credit, newId, request.CardName);
        return _repository.Add(card);
    }

    public CardDetailResponse? GetById(int cardId)
    {
        var card = _repository.GetById(cardId);
        if (card == null) return null;

        var maskedNumber = CardMaskingHelper.MaskCardNumber(card.CardNumber);
        return new CardDetailResponse(
            card.Id,
            maskedNumber,
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

    public CreditCardDetailResponse? GetCreditById(int cardId)
    {
        var card = _repository.GetById(cardId);
        if (card == null || card is not CreditCard creditCard) return null;

        var maskedNumber = CardMaskingHelper.MaskCardNumber(creditCard.CardNumber);

        var nextClosing = DateHelper.GetNextOccurrence(creditCard.ClosingDay);
        var nextDue = DateHelper.GetNextOccurrence(creditCard.DueDateDay);

        return new CreditCardDetailResponse(
            creditCard.Id,
            maskedNumber,
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
                CardMaskingHelper.MaskCardNumber(c.CardNumber),
                c.CardType,
                c.CardNetwork,
                c.BankId,
                c.CardName
            ))
            .ToList();
    }

    public bool UpdateCardName(int cardId, string cardName)
    {
        var card = _repository.GetById(cardId);
        if (card == null) return false;
        
        card.UpdateCardName(cardName);

        return _repository.Update(card);
    }

    public bool UpdateCreditCardDates(int cardId, UpdateCreditCardRequest request)
    {
        var card = _repository.GetById(cardId);
        if (card == null || card is not CreditCard creditCard) return false;
        creditCard.UpdateClosingDay(request.ClosingDay);
        creditCard.UpdateDueDateDay(request.DueDateDay);
        
        return _repository.Update(creditCard);
    }

    public bool Activate(int cardId)
    {
        var card = _repository.GetById(cardId);
        if (card == null) return false;
        card.SetIsActive(true);
        return _repository.Update(card);
    }

    public bool Deactivate(int cardId)
    {
        var card = _repository.GetById(cardId);
        if (card == null) return false;
        card.SetIsActive(false);
        return _repository.Update(card);
    }
}