using FinancesControl.API.Enums;

namespace FinancesControl.API.Models;

public class GeneralTransaction : Transaction
{
    public GeneralTransaction(decimal amount, PaymentMethod paymentMethod, TransactionType transactionType, int categoryId, int bankId) : base(amount, paymentMethod, transactionType, categoryId)
    {
        BankId = bankId;
    }

    public int BankId { get; private set; }
    public Bank? Bank { get; private set; }
}
