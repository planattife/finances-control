using FinancesControl.API.Common;
using FinancesControl.API.Enums;
using FinancesControl.API.Models;

namespace FinancesControl.API.Banks;

public class Bank : BaseModel
{
    public Bank(string name, BankAccountType accountType, int id) : base(id)
    {
        ValidationHelper.ValidateName(name, 2 , 50);
        Name = name;
        AccountType = accountType;
        IsActive = true;
    }

    public string Name { get; private set; }
    public BankAccountType AccountType { get; private set; }
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
}
