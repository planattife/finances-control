using System.Collections.Generic;
using System.Linq;
using FinancesControl.API.Models;

namespace FinancesControl.API.Cards.Repository;

public class CardRepository : ICardRepository
{
    private static readonly List<Card> _cards = new();

    public int Add(Card card)
    {
        _cards.Add(card);
        return card.Id;
    }

    public Card? GetById(int cardId)
    {
        return _cards.FirstOrDefault(c => c.Id == cardId);
    }

    public List<Card> GetAll()
    {
        return _cards.ToList();
    }

    public bool Update(Card card)
    {
        var idx = _cards.FindIndex(c => c.Id == card.Id);
        if (idx == -1) return false;
        _cards[idx] = card;
        return true;
    }
}