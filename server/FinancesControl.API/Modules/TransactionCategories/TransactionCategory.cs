using FinancesControl.API.Common;

namespace FinancesControl.API.Models;

public class TransactionCategory : BaseModel
{
    public TransactionCategory(string name, int id) : base(id)
    {
        ValidationHelper.ValidateName(name);
        Name = name;
        IsActive = true;
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
}
