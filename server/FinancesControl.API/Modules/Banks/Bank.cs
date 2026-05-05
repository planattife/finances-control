using FinancesControl.API.Common;
using FinancesControl.API.Enums;
using FinancesControl.API.Models;

namespace FinancesControl.API.Banks;

public class Bank : BaseModel
{
    // Public constructor - for creating NEW banks
    public Bank(string name, BankAccountType accountType, int id, string userId) : base(id)
    {
        ValidationHelper.ValidateName(name, 2 , 50);
        Name = name;
        AccountType = accountType;
        IsActive = true;
        UserId = userId;
    }

    // Private constructor - for DB reconstruction
    private Bank(int id, string name, BankAccountType accountType, string userId, bool isActive,
        DateTime createdAt, DateTime? updatedAt)
        : base(id, createdAt, updatedAt)
    {
        Name = name;
        AccountType = accountType;
        IsActive = isActive;
        UserId = userId;
    }

    public string Name { get; private set; }
    public BankAccountType AccountType { get; private set; }
    public bool IsActive { get; private set; }
    public string UserId { get; private set; }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    // Factory method for DB reconstruction
    internal static Bank FromDatabase(int id, string name, BankAccountType accountType, string userId,
        bool isActive, DateTime createdAt, DateTime? updatedAt)
    {
        return new Bank(id, name, accountType, userId, isActive, createdAt, updatedAt);
    }
}
