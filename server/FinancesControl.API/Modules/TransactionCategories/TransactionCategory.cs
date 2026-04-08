using FinancesControl.API.Common;

namespace FinancesControl.API.Models;

public class TransactionCategory : BaseModel
{
    // Public constructor - for creating NEW categories
    public TransactionCategory(string name, int id) : base(id)
    {
        ValidationHelper.ValidateName(name);
        Name = name;
        IsActive = true;
    }

    // Private constructor - for DB reconstruction
    private TransactionCategory(int id, string name, bool isActive,
        DateTime createdAt, DateTime? updatedAt)
        : base(id, createdAt, updatedAt)
    {
        Name = name;
        IsActive = isActive;
    }

    public string Name { get; private set; }
    public bool IsActive { get; private set; }

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
    internal static TransactionCategory FromDatabase(int id, string name, bool isActive,
        DateTime createdAt, DateTime? updatedAt)
    {
        return new TransactionCategory(id, name, isActive, createdAt, updatedAt);
    }
}

