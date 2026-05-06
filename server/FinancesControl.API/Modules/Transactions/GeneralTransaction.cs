using FinancesControl.API.Banks;
using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;

public class GeneralTransaction : Transaction
{
    public GeneralTransaction(decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, DateTime transactionDate, long categoryId, long bankId, long accountId, long id, string? description = null) 
    : base(amount, paymentMethod, transactionType, transactionDate, categoryId, accountId, id, description)
    {
        BankId = bankId;
    }

    protected GeneralTransaction(long id, decimal amount, PaymentMethod paymentMethod, TransactionType transactionType,
        DateTime transactionDate, long categoryId, long accountId, long bankId, bool isActive,
        DateTime createdAt, DateTime? updatedAt, string? description = null)
        : base(id, amount, paymentMethod, transactionType, transactionDate, categoryId, accountId, isActive, createdAt, updatedAt, description)
    {
        BankId = bankId;
    }

    public long BankId { get; private set; }

    internal static GeneralTransaction FromDatabase(long id, decimal amount, PaymentMethod paymentMethod,
        TransactionType transactionType, DateTime transactionDate, long categoryId, long accountId, long bankId,
        bool isActive, DateTime createdAt, DateTime? updatedAt, string? description = null)
    {
        return new GeneralTransaction(id, amount, paymentMethod, transactionType, transactionDate, categoryId,
            accountId, bankId, isActive, createdAt, updatedAt, description);
    }
}
