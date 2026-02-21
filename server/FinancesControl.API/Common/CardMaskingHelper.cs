namespace FinancesControl.API.Common;

public static class CardMaskingHelper
{
    public static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
            return "****";
        
        return $"****{cardNumber.Substring(cardNumber.Length - 4)}";
    }
}