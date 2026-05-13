using FinancesControl.API.Models;

namespace FinancesControl.API.Cards.Repository;

public interface ICardRepository
{
    long Add(Card card);
    Card? GetById(long cardId, long accountId);
    List<Card> GetAll(long accountId);
    bool Update(Card card, long accountId);
}