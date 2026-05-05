using FinancesControl.API.Enums;
using FinancesControl.API.Models;

namespace FinancesControl.API.Models;

public class CreditCard : Card
{
    // Public constructor - for creating NEW credit cards
    public CreditCard(string cardNumber, DateTime expirationDate, CardNetwork cardNetwork, int bankId, int closingDay, int dueDateDay, CardType cardType = CardType.Credit, int id = 0, string? cardName = null) 
    : base(cardNumber, expirationDate, cardNetwork, bankId, cardType, id, cardName)
    {
        ClosingDay = closingDay;
        DueDateDay = dueDateDay;
    }

    // Private constructor - for DB reconstruction
    private CreditCard(int id, string cardNumber, DateTime expirationDate, CardNetwork cardNetwork,
        int bankId, int closingDay, int dueDateDay, bool isActive, string? cardName,
        DateTime createdAt, DateTime? updatedAt)
        : base(id, cardNumber, expirationDate, cardNetwork, bankId, CardType.Credit, isActive, cardName, createdAt, updatedAt)
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
    internal static CreditCard FromDatabase(int id, string cardNumber, DateTime expirationDate,
        CardNetwork cardNetwork, int bankId, int closingDay, int dueDateDay, bool isActive, string? cardName,
        DateTime createdAt, DateTime? updatedAt)
    {
        return new CreditCard(id, cardNumber, expirationDate, cardNetwork, bankId, closingDay, dueDateDay, isActive, cardName, createdAt, updatedAt);
    }
}

