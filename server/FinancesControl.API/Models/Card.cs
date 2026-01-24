using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;

public class Card : BaseModel
{
    public Card(string cardNumber, DateTime expirationDate, CardNetwork cardNetwork, int bankId, CardType cardType)
    {
        CardNumber = cardNumber;
        ExpirationDate = expirationDate;
        CardNetwork = cardNetwork;
        BankId = bankId;

        IsActive = true;
        CardType = cardType;
    }

    public CardType CardType { get; private set; }
    public string CardNumber { get; private set; }
    public DateTime ExpirationDate { get; private set; }
    public bool IsActive { get; private set; }
    public CardNetwork CardNetwork { get; private set; }
    public int BankId { get; private set; }
    public Bank? Bank { get; private set; }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}
