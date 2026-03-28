using FinancesControl.API.Models;

namespace FinancesControl.API.Cards.Repository;

public interface ICardRepository
{
    int Add(Card card);
    Card? GetById(int cardId);
    List<Card> GetAll();
    bool Update(Card card);
}