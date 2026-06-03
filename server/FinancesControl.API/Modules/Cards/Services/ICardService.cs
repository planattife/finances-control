using FinancesControl.API.Cards.DTOs;
using FinancesControl.API.Common;

namespace FinancesControl.API.Cards.Services;

public interface ICardService
{
    Task<long> CreateDebitCard(CreateDebitCardRequest request, long accountId, string userId);
    Task<long> CreateCreditCard(CreateCreditCardRequest request, long accountId, string userId);
    Task<SingleResponse<CardDetailResponse>> GetById(long cardId, long accountId, string userId);
    Task<SingleResponse<CreditCardDetailResponse>> GetCreditById(long cardId, long accountId, string userId);
    Task<PaginatedResponse<CardListItemResponse>> GetAll(long accountId, string userId, PaginationParams paginationParams);
    Task<bool> UpdateCardName(long cardId, long accountId, string userId, string cardName);
    Task<bool> UpdateCreditCardDates(long cardId, long accountId, string userId, UpdateCreditCardRequest request);
    Task<bool> Activate(long cardId, long accountId, string userId);
    Task<bool> Deactivate(long cardId, long accountId, string userId);
    Task<bool> ExistsAsync(long cardId, long accountId);
}
