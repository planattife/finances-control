using FinancesControl.API.Enums;
using FinancesControl.API.Common;

namespace FinancesControl.API.Models;


public class Transaction : BaseModel
{
    public Transaction(decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, DateTime transactionDate, long categoryId, long accountId, long id, string? description = null, string categoryName = "") : base(id)
    {
        ValidateAmount(amount);
        ValidateCategoryId(categoryId);
        ValidateAccountId(accountId);
        Amount = amount;
        PaymentMethod = paymentMethod;
        TransactionType = transactionType;
        TransactionDate = transactionDate;
        CategoryId = categoryId;
        CategoryName = categoryName;
        AccountId = accountId;
        Description = description;
        IsActive = true;
    }

    protected Transaction(long id, decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, 
        DateTime transactionDate, long categoryId, string categoryName, long accountId, bool isActive,
        DateTime createdAt, DateTime? updatedAt, string? description = null)
        : base(id, createdAt, updatedAt)
    {
        Amount = amount;
        PaymentMethod = paymentMethod;
        TransactionType = transactionType;
        TransactionDate = transactionDate;
        CategoryId = categoryId;
        CategoryName = categoryName;
        AccountId = accountId;
        IsActive = isActive;
        Description = description;
    }

    public DateTime TransactionDate { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public long CategoryId { get; private set; }
    public string CategoryName { get; private set; }
    public string? Description { get; private set; }
    public long AccountId { get; private set; }
    public bool IsActive { get; private set; }
    private void ValidateAccountId(long accountId)
    {
        if (accountId <= 0)
        {
            throw new DomainValidationException("An account must be provided.");
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    public void ChangeAmount(decimal newAmount)
    {
        ValidateAmount(newAmount);
        Amount = newAmount;
        Touch();
    }

    public void ChangeCategory(long newCategoryId)
    {
        ValidateCategoryId(newCategoryId);
        CategoryId = newCategoryId;
        Touch();
    }

    public void ChangeDescription(string? newDescription)
    {
        Description = newDescription;
        Touch();
    }

    private void ValidateAmount(decimal amount)
    {
        if (amount < 0)
        {
            throw new DomainValidationException("Amount cannot be less than 0.");
        }
    }

    private void ValidateCategoryId(long categoryId)
    {
        if (categoryId <= 0)
        {
            throw new DomainValidationException("CategoryId must be greater than 0.");
        }
    }

    internal static Transaction FromDatabase(long id, decimal amount, PaymentMethod paymentMethod, 
        TransactionType transactionType, DateTime transactionDate, long categoryId, string categoryName, long accountId, 
        bool isActive, DateTime createdAt, DateTime? updatedAt, string? description = null)
    {
        return new Transaction(id, amount, paymentMethod, transactionType, transactionDate, categoryId, 
            categoryName, accountId, isActive, createdAt, updatedAt, description);
    }
}
