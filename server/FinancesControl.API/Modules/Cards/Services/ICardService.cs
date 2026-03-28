using FinancesControl.API.Cards.DTOs;

namespace FinancesControl.API.Cards.Services;

public interface ICardService
{
    int CreateDebitCard(CreateDebitCardRequest request);
    int CreateCreditCard(CreateCreditCardRequest request);
    CardDetailResponse? GetById(int cardId);
    CreditCardDetailResponse? GetCreditById(int cardId);
    List<CardListItemResponse> GetAll();
    bool UpdateCardName(int cardId, string cardName);
    bool UpdateCreditCardDates(int cardId, UpdateCreditCardRequest request);
    bool Activate(int cardId);
    bool Deactivate(int cardId);
}