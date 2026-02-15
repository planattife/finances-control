using FinancesControl.API.Common;
using FinancesControl.API.Models;

namespace FinancesControl.API.Accounts;

public class Account : BaseModel
{
    public Account(int id, string name, string userId) : base(id)
    {
        ValidationHelper.ValidateName(name, 2, 50);
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
        ValidationHelper.ValidateName(newName, 2, 50);
        Name = newName;
        Touch();
    }
}
