namespace CurrencyTelegramBot.Models.Models;

public class MinorCurrency
{
    public string BaseCurrency = string.Empty;
    public string Currency = string.Empty;
    public double SaleRate { get; set; }
    public double PurchaseRate { get; set; }
}