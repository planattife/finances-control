using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;

public class Bank : BaseModel
{
    public Bank(string name, BankAccountType accountType, int id) : base(id)
    {
        Name = name;
        AccountType = accountType;
        IsActive = true;
    }

    public string Name { get; private set; }
    public BankAccountType AccountType { get; private set; }
    public bool IsActive { get; private set; }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}
