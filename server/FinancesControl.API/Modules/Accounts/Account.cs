using FinancesControl.API.Common;
using FinancesControl.API.Models;

namespace FinancesControl.API.Accounts;

public class Account : BaseModel
{
    // Public constructor - for creating NEW accounts
    public Account(long id, string name, string userId) : base(id)
    {
        ValidationHelper.ValidateName(name, 2, 50);
        Name = name;
        UserId = userId;
        IsActive = true;
    }

    // Private constructor - for DB reconstruction
    private Account(long id, string name, string userId, bool isActive, 
        DateTime createdAt, DateTime? updatedAt) 
        : base(id, createdAt, updatedAt)
    {
        Name = name;
        UserId = userId;
        IsActive = isActive;
    }

    public string Name { get; private set; }
    public string UserId { get; private set; }
    public bool IsActive { get; private set; }

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

    public void Rename(string newName)
    {
        ValidationHelper.ValidateName(newName, 2, 50);
        Name = newName;
        Touch();
    }

    // Factory method for DB reconstruction
    internal static Account FromDatabase(long id, string name, string userId, bool isActive,
        DateTime createdAt, DateTime? updatedAt)
    {
        return new Account(id, name, userId, isActive, createdAt, updatedAt);
    }
}
