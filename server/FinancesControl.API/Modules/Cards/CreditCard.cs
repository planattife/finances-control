using FinancesControl.API.Enums;
using FinancesControl.API.Models;

namespace FinancesControl.API.Models;

public class CreditCard : Card
{
    public CreditCard(string cardNumber, DateTime expirationDate, CardNetwork cardNetwork, int bankId, int closingDay, int dueDateDay, CardType cardType = CardType.Credit, int id = 0, string? cardName = null) 
    : base(cardNumber, expirationDate, cardNetwork, bankId, cardType, id, cardName)
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
}
