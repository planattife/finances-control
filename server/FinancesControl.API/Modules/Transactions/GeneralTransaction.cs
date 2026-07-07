using FinancesControl.API.Banks;
using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;

public class GeneralTransaction : Transaction
{
    public GeneralTransaction(decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, DateTime transactionDate, long categoryId, long bankId, long accountId, long id, string? description = null, string? bankName = null) 
    : base(amount, paymentMethod, transactionType, transactionDate, categoryId, accountId, id, description)
    {
        BankId = bankId;
        BankName = bankName;
    }

    protected GeneralTransaction(long id, decimal amount, PaymentMethod paymentMethod, TransactionType transactionType,
        DateTime transactionDate, long categoryId, string categoryName, long accountId, long bankId, string? bankName, bool isActive,
        DateTime createdAt, DateTime? updatedAt, string? description = null)
        : base(id, amount, paymentMethod, transactionType, transactionDate, categoryId, categoryName, accountId, isActive, createdAt, updatedAt, description)
    {
        BankId = bankId;
        BankName = bankName;
    }

    public long BankId { get; private set; }
    public string? BankName { get; private set; }

    internal static GeneralTransaction FromDatabase(long id, decimal amount, PaymentMethod paymentMethod,
        TransactionType transactionType, DateTime transactionDate, long categoryId, string categoryName, long accountId, long bankId, string? bankName,
        bool isActive, DateTime createdAt, DateTime? updatedAt, string? description = null)
    {
        return new GeneralTransaction(id, amount, paymentMethod, transactionType, transactionDate, categoryId,
            categoryName, accountId, bankId, bankName, isActive, createdAt, updatedAt, description);
    }
}
