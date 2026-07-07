
using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;

public class CardTransaction : Transaction
{
    public CardTransaction(decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, DateTime transactionDate, long categoryId, long cardId, long accountId, long id, string? description = null, string? cardName = null) 
    : base(amount, paymentMethod, transactionType, transactionDate, categoryId, accountId, id, description)
    {
        CardId = cardId;
        CardName = cardName;
    }

    protected CardTransaction(long id, decimal amount, PaymentMethod paymentMethod, TransactionType transactionType,
        DateTime transactionDate, long categoryId, string categoryName, long accountId, long cardId, string? cardName, bool isActive,
        DateTime createdAt, DateTime? updatedAt, string? description = null)
        : base(id, amount, paymentMethod, transactionType, transactionDate, categoryId, categoryName, accountId, isActive, createdAt, updatedAt, description)
    {
        CardId = cardId;
        CardName = cardName;
    }

    public long CardId { get; private set; }
    public string? CardName { get; private set; }

    internal static CardTransaction FromDatabase(long id, decimal amount, PaymentMethod paymentMethod,
        TransactionType transactionType, DateTime transactionDate, long categoryId, string categoryName, long accountId, long cardId, string? cardName,
        bool isActive, DateTime createdAt, DateTime? updatedAt, string? description = null)
    {
        return new CardTransaction(id, amount, paymentMethod, transactionType, transactionDate, categoryId,
            categoryName, accountId, cardId, cardName, isActive, createdAt, updatedAt, description);
    }
}
