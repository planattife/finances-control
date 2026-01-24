
using Microsoft.AspNetCore.Identity;

namespace FinancesControl.API.Models;

public class Account : BaseModel
{
    public Account(string name, IdentityUser user)
    {
        Name = name;
        User = user;
        UserId = user.Id;
        IsActive = true;
    }

    public string Name { get; private set; }
    public string UserId { get; private set; }
    public IdentityUser User { get; private set; }
    public bool IsActive { get; private set; }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}
