using FinancesControl.API.Banks;
using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;

public class GeneralTransaction : Transaction
{
    public GeneralTransaction(decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, DateTime transactionDate, int categoryId, int bankId, int accountId, int id) 
    : base(amount, paymentMethod, transactionType, transactionDate, categoryId, accountId, id)
    {
        BankId = bankId;
    }

    protected GeneralTransaction(int id, decimal amount, PaymentMethod paymentMethod, TransactionType transactionType,
        DateTime transactionDate, int categoryId, int accountId, int bankId, bool isActive,
        DateTime createdAt, DateTime? updatedAt)
        : base(id, amount, paymentMethod, transactionType, transactionDate, categoryId, accountId, isActive, createdAt, updatedAt)
    {
        BankId = bankId;
    }

    public int BankId { get; private set; }

    internal static GeneralTransaction FromDatabase(int id, decimal amount, PaymentMethod paymentMethod,
        TransactionType transactionType, DateTime transactionDate, int categoryId, int accountId, int bankId,
        bool isActive, DateTime createdAt, DateTime? updatedAt)
    {
        return new GeneralTransaction(id, amount, paymentMethod, transactionType, transactionDate, categoryId,
            accountId, bankId, isActive, createdAt, updatedAt);
    }
}
