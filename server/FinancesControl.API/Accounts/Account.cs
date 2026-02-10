using FinancesControl.API.Models;

namespace FinancesControl.API.Accounts;

public class Account : BaseModel
{
    private const int MinNameLength = 2;
    private const int MaxNameLength = 50;

    public Account(int id, string name, string userId) : base(id)
    {
        ValidateName(name);
        Name = name;
        UserId = userId;
        IsActive = true;
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
        ValidateName(newName);
        Name = newName;
        Touch();
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.");

        if (name.Length < MinNameLength)
            throw new ArgumentException($"Name must be at least {MinNameLength} characters long.");

        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Name cannot exceed {MaxNameLength} characters.");
    }
}
