namespace FinancesControl.API.Models;

public class BaseModel
{
    public BaseModel(int id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
    }
    
    public int Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public void Touch()
    {
        UpdatedAt = DateTime.Now;
    }
}
