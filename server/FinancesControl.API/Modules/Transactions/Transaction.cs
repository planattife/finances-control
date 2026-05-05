using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;


public class Transaction : BaseModel
{
    public Transaction(decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, DateTime transactionDate, long categoryId, long accountId, long id) : base(id)
    {
        ValidateAmount(amount);
        ValidateCategoryId(categoryId);
        ValidateAccountId(accountId);
        Amount = amount;
        PaymentMethod = paymentMethod;
        TransactionType = transactionType;
        TransactionDate = transactionDate;
        CategoryId = categoryId;
        AccountId = accountId;
        IsActive = true;
    }

    protected Transaction(long id, decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, 
        DateTime transactionDate, long categoryId, long accountId, bool isActive,
        DateTime createdAt, DateTime? updatedAt)
        : base(id, createdAt, updatedAt)
    {
        Amount = amount;
        PaymentMethod = paymentMethod;
        TransactionType = transactionType;
        TransactionDate = transactionDate;
        CategoryId = categoryId;
        AccountId = accountId;
        IsActive = isActive;
    }

    public DateTime TransactionDate { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public long CategoryId { get; private set; }

    public long AccountId { get; private set; }
    public bool IsActive { get; private set; }
    private void ValidateAccountId(long accountId)
    {
        if (accountId <= 0)
        {
            throw new ArgumentException("An account must be provided.", nameof(accountId));
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

    private void ValidateAmount(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be less than 0", nameof(amount));
        }
    }

    private void ValidateCategoryId(long categoryId)
    {
        if (categoryId <= 0)
        {
            throw new ArgumentException("CategoryId must be greater than 0", nameof(categoryId));
        }
    }

    internal static Transaction FromDatabase(long id, decimal amount, PaymentMethod paymentMethod, 
        TransactionType transactionType, DateTime transactionDate, long categoryId, long accountId, 
        bool isActive, DateTime createdAt, DateTime? updatedAt)
    {
        return new Transaction(id, amount, paymentMethod, transactionType, transactionDate, categoryId, 
            accountId, isActive, createdAt, updatedAt);
    }
}
