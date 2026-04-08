namespace FinancesControl.API.Models;

public class BaseModel
{
    // Public constructor - for creating NEW entities
    public BaseModel(int id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
    }

    // Protected constructor - for DB reconstruction
    protected BaseModel(int id, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
    
    public int Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
