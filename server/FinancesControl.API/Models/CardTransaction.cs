
using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;

public class CardTransaction : Transaction
{
    public CardTransaction(decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, int categoryId, int cardId, int id) 
    : base(amount, paymentMethod, transactionType, categoryId, id)
    {
        CardId = cardId;
    }

    public int CardId { get; private set; }
    public Card? Card { get; private set; }
}
