using CurrencyTelegramBot.Models.Models;

namespace CurrencyTelegramBot.Services.Interfaces;

public interface IExchangeRateService
{
    Task<string> GetAllRates(BaseCurrency baseCurrency);
    Task<string> GetSelectedRates(BaseCurrency baseCurrency, string currencyLit);
    Task<BaseCurrency> GetBaseCurrency(DateTime date);
    Task<List<string>> GetAvailableRates(BaseCurrency baseCurrency);
}