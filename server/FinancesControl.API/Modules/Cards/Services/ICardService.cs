using FinancesControl.API.Cards.DTOs;

namespace FinancesControl.API.Cards.Services;

public interface ICardService
{
    long CreateDebitCard(CreateDebitCardRequest request);
    long CreateCreditCard(CreateCreditCardRequest request);
    CardDetailResponse? GetById(long cardId);
    CreditCardDetailResponse? GetCreditById(long cardId);
    List<CardListItemResponse> GetAll();
    bool UpdateCardName(long cardId, string cardName);
    bool UpdateCreditCardDates(long cardId, UpdateCreditCardRequest request);
    bool Activate(long cardId);
    bool Deactivate(long cardId);
}