using FinancesControl.API.Cards.DTOs;

namespace FinancesControl.API.Cards.Services;

public interface ICardService
{
    long CreateDebitCard(CreateDebitCardRequest request, long accountId, string userId);
    long CreateCreditCard(CreateCreditCardRequest request, long accountId, string userId);
    CardDetailResponse? GetById(long cardId, long accountId, string userId);
    CreditCardDetailResponse? GetCreditById(long cardId, long accountId, string userId);
    List<CardListItemResponse> GetAll(long accountId, string userId);
    bool UpdateCardName(long cardId, long accountId, string userId, string cardName);
    bool UpdateCreditCardDates(long cardId, long accountId, string userId, UpdateCreditCardRequest request);
    bool Activate(long cardId, long accountId, string userId);
    bool Deactivate(long cardId, long accountId, string userId);
}