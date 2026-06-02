using FinancesControl.API.Models;

namespace FinancesControl.API.Cards.Repository;

public interface ICardRepository
{
    Task<long> Add(Card card);
    Task<Card?> GetById(long cardId, long accountId);
    Task<List<Card>> GetAll(long accountId);
    Task<bool> Update(Card card, long accountId);
    Task<bool> ExistsAsync(long cardId, long accountId);
}