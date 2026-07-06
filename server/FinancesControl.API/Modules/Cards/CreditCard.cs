using FinancesControl.API.Enums;
using FinancesControl.API.Models;

namespace FinancesControl.API.Models;

public class CreditCard : Card
{
    // Public constructor - for creating NEW credit cards
    public CreditCard(string cardNumber, DateTime expirationDate, CardNetwork cardNetwork, long bankId, long accountId, int closingDay, int dueDateDay, CardType cardType = CardType.Credit, long id = 0, string? cardName = null, string bankName = "") 
    : base(cardNumber, expirationDate, cardNetwork, bankId, accountId, cardType, id, cardName, bankName)
    {
        ClosingDay = closingDay;
        DueDateDay = dueDateDay;
    }

    // Private constructor - for DB reconstruction
    private CreditCard(long id, string cardNumber, DateTime expirationDate, CardNetwork cardNetwork,
        long bankId, long accountId, int closingDay, int dueDateDay, bool isActive, string? cardName,
        DateTime createdAt, DateTime? updatedAt, string bankName = "")
        : base(id, cardNumber, expirationDate, cardNetwork, bankId, accountId, CardType.Credit, isActive, cardName, createdAt, updatedAt, bankName)
    {
        ClosingDay = closingDay;
        DueDateDay = dueDateDay;
    }

    public int ClosingDay { get; private set; }
    public int DueDateDay { get; private set; }

    public void UpdateClosingDay(int closingDay)
    {
        ClosingDay = closingDay;
        Touch();
    }

    public void UpdateDueDateDay(int dueDateDay)
    {
        DueDateDay = dueDateDay;
        Touch();
    }

    // Factory method for DB reconstruction
    internal static CreditCard FromDatabase(long id, string cardNumber, DateTime expirationDate,
        CardNetwork cardNetwork, long bankId, long accountId, int closingDay, int dueDateDay, bool isActive, string? cardName,
        DateTime createdAt, DateTime? updatedAt, string bankName = "")
    {
        return new CreditCard(id, cardNumber, expirationDate, cardNetwork, bankId, accountId, closingDay, dueDateDay, isActive, cardName, createdAt, updatedAt, bankName);
    }
}

