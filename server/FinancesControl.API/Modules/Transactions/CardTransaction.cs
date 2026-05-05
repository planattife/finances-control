
using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;

public class CardTransaction : Transaction
{
    public CardTransaction(decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, DateTime transactionDate, long categoryId, long cardId, long accountId, long id) 
    : base(amount, paymentMethod, transactionType, transactionDate, categoryId, accountId, id)
    {
        CardId = cardId;
    }

    protected CardTransaction(long id, decimal amount, PaymentMethod paymentMethod, TransactionType transactionType,
        DateTime transactionDate, long categoryId, long accountId, long cardId, bool isActive,
        DateTime createdAt, DateTime? updatedAt)
        : base(id, amount, paymentMethod, transactionType, transactionDate, categoryId, accountId, isActive, createdAt, updatedAt)
    {
        CardId = cardId;
    }

    public long CardId { get; private set; }

    internal static CardTransaction FromDatabase(long id, decimal amount, PaymentMethod paymentMethod,
        TransactionType transactionType, DateTime transactionDate, long categoryId, long accountId, long cardId,
        bool isActive, DateTime createdAt, DateTime? updatedAt)
    {
        return new CardTransaction(id, amount, paymentMethod, transactionType, transactionDate, categoryId,
            accountId, cardId, isActive, createdAt, updatedAt);
    }
}
