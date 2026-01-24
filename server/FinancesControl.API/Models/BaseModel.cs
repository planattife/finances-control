namespace FinancesControl.API.Models;

public class BaseModel
{
    public BaseModel()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
    }
    
    public int Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public void SetUpdatedDate(DateTime updatedAt)
    {
        UpdatedAt = updatedAt;
    }
}
