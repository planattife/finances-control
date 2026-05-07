using FinancesControl.API.Common;

namespace FinancesControl.API.Models;

public class TransactionCategory : BaseModel
{
    // Public constructor - for creating NEW categories
    public TransactionCategory(string name, long id, string? userId) : base(id)
    {
        ValidationHelper.ValidateName(name);
        Name = name;
        UserId = userId;
        IsActive = true;
        IsDefault = false;
    }

    // Private constructor - for DB reconstruction
    private TransactionCategory(long id, string name, bool isActive, bool isDefault, string? userId,
        DateTime createdAt, DateTime? updatedAt)
        : base(id, createdAt, updatedAt)
    {
        Name = name;
        IsActive = isActive;
        IsDefault = isDefault;
        UserId = userId;
    }

    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDefault { get; private set; }
    public string? UserId { get; private set; }

    public void Rename(string newName)
    {
        ValidationHelper.ValidateName(newName);
        Name = newName;
        Touch();
    }

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
    internal static TransactionCategory FromDatabase(long id, string name, bool isActive, bool isDefault, string? userId,
        DateTime createdAt, DateTime? updatedAt)
    {
        return new TransactionCategory(id, name, isActive, isDefault, userId, createdAt, updatedAt);
    }
}

