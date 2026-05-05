
using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;

public class CardTransaction : Transaction
{
    public CardTransaction(decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, DateTime transactionDate, int categoryId, int cardId, int accountId, int id) 
    : base(amount, paymentMethod, transactionType, transactionDate, categoryId, accountId, id)
    {
        CardId = cardId;
    }

    protected CardTransaction(int id, decimal amount, PaymentMethod paymentMethod, TransactionType transactionType,
        DateTime transactionDate, int categoryId, int accountId, int cardId, bool isActive,
        DateTime createdAt, DateTime? updatedAt)
        : base(id, amount, paymentMethod, transactionType, transactionDate, categoryId, accountId, isActive, createdAt, updatedAt)
    {
        CardId = cardId;
    }

    public int CardId { get; private set; }

    internal static CardTransaction FromDatabase(int id, decimal amount, PaymentMethod paymentMethod,
        TransactionType transactionType, DateTime transactionDate, int categoryId, int accountId, int cardId,
        bool isActive, DateTime createdAt, DateTime? updatedAt)
    {
        return new CardTransaction(id, amount, paymentMethod, transactionType, transactionDate, categoryId,
            accountId, cardId, isActive, createdAt, updatedAt);
    }
}
