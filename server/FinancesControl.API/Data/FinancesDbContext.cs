using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinancesControl.API.Data;

public class FinancesDbContext : IdentityDbContext
{
    public FinancesDbContext(DbContextOptions<FinancesDbContext> options) : base(options)
    {
        
    }
}
