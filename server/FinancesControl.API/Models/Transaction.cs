using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;

public class Transaction : BaseModel
{
    public Transaction(decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, int categoryId)
    {
        Amount = amount;
        PaymentMethod = paymentMethod;
        TransactionType = transactionType;
        CategoryId = categoryId;
    }

    public DateTime TransactionDate { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public int CategoryId { get; private set; }
    public TransactionCategory? Category { get; private set; }
}
