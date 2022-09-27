namespace CurrencyTelegramBot.Models.Models;

public class BaseCurrency
{
    public string BaseCurrencyLit = string.Empty;
    public DateTime Date;
    public List<MinorCurrency> ExchangeRate = new();
}