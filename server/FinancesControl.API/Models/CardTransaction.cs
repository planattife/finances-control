
using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;

public class CardTransaction : Transaction
{
    public CardTransaction(decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, int categoryId, int cardId) 
    : base(amount, paymentMethod, transactionType, categoryId)
    {
        CardId = cardId;
    }

    public int CardId { get; private set; }
    public Card? Card { get; private set; }
}
