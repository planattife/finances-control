using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;

public class CreditCard : Card
{
    public CreditCard(string cardNumber, DateTime expirationDate, CardNetwork cardNetwork, int bankId, int closingDay, int dueDateDay, CardType cardType = CardType.Credit) 
    : base(cardNumber, expirationDate, cardNetwork, bankId, cardType)
    {
        ClosingDay = closingDay;
        DueDateDay = dueDateDay;
    }

    public int ClosingDay { get; private set; }
    public int DueDateDay { get; private set; }

    public void UpdateClosingDay(int closingDay)
    {
        ClosingDay = closingDay;
    }
    public void UpdateDueDateDay(int dueDateDay)
    {
        DueDateDay = dueDateDay;
    }
}
