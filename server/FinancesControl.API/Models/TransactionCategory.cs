namespace FinancesControl.API.Models;

public class TransactionCategory : BaseModel
{
    public TransactionCategory(string name, int id) : base(id)
    {
        Name = name;
        IsActive = true;
    }

    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}
