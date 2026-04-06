
using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;

public class CardTransaction : Transaction
{
    public CardTransaction(decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, DateTime transactionDate, int categoryId, int cardId, int accountId, int id) 
    : base(amount, paymentMethod, transactionType, transactionDate, categoryId, accountId, id)
    {
        CardId = cardId;
    }

    public int CardId { get; private set; }
}
