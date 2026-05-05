using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;

public class Card : BaseModel
{
    // Public constructor - for creating NEW cards
    public Card(string cardNumber, DateTime expirationDate, CardNetwork cardNetwork, long bankId, CardType cardType, long id, string? cardName = null) : base(id)
    {
        CardNumber = cardNumber;
        ExpirationDate = expirationDate;
        CardNetwork = cardNetwork;
        BankId = bankId;
        CardName = cardName;

        IsActive = true;
        CardType = cardType;
    }

    // Protected constructor - for DB reconstruction (protected so CreditCard can use it)
    protected Card(long id, string cardNumber, DateTime expirationDate, CardNetwork cardNetwork, 
        long bankId, CardType cardType, bool isActive, string? cardName,
        DateTime createdAt, DateTime? updatedAt)
        : base(id, createdAt, updatedAt)
    {
        CardNumber = cardNumber;
        ExpirationDate = expirationDate;
        CardNetwork = cardNetwork;
        BankId = bankId;
        CardName = cardName;
        IsActive = isActive;
        CardType = cardType;
    }

    public CardType CardType { get; private set; }
    public string CardNumber { get; private set; }
    public DateTime ExpirationDate { get; private set; }
    public bool IsActive { get; private set; }
    public CardNetwork CardNetwork { get; private set; }
    public long BankId { get; private set; }
    public string? CardName { get; private set; }
    
    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
        Touch();
    }

    public void UpdateCardName(string cardName) {
        CardName = cardName;
        Touch();
    }

    // Factory method for DB reconstruction
    internal static Card FromDatabase(long id, string cardNumber, DateTime expirationDate, 
        CardNetwork cardNetwork, long bankId, CardType cardType, bool isActive, string? cardName,
        DateTime createdAt, DateTime? updatedAt)
    {
        return new Card(id, cardNumber, expirationDate, cardNetwork, bankId, cardType, isActive, cardName, createdAt, updatedAt);
    }
}
