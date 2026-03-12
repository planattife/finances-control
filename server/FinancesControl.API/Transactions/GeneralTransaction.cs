using FinancesControl.API.Banks;
using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;

public class GeneralTransaction : Transaction
{
    public GeneralTransaction(decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, DateTime transactionDate, int categoryId, int bankId, int id) : base(amount, paymentMethod, transactionType, transactionDate, categoryId, id)
    {
        BankId = bankId;
    }

    public int BankId { get; private set; }
}
