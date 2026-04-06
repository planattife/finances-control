using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;


public class Transaction : BaseModel
{
    public Transaction(decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, DateTime transactionDate, int categoryId, int accountId, int id) : base(id)
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

    public DateTime TransactionDate { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public int CategoryId { get; private set; }

    public int AccountId { get; private set; }
    public bool IsActive { get; private set; }
    private void ValidateAccountId(int accountId)
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

    public void ChangeCategory(int newCategoryId)
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

    private void ValidateCategoryId(int categoryId)
    {
        if (categoryId <= 0)
        {
            throw new ArgumentException("CategoryId must be greater than 0", nameof(categoryId));
        }
    }
}
