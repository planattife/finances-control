using FinancesControl.API.Models;

namespace FinancesControl.API.Cards.Repository;

public interface ICardRepository
{
    long Add(Card card);
    Card? GetById(long cardId);
    List<Card> GetAll();
    bool Update(Card card);
}