using System.Text;
using CurrencyTelegramBot.Models.Models;
using CurrencyTelegramBot.Services.Interfaces;
using CurrencyTelegramBot.Services.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CurrencyTelegramBot.Services.Implementations;

public class ExchangeRateService : IExchangeRateService
{
    private const string ApiKey = "PrivatBankApiJson";

    public Task<string> GetAllRates(BaseCurrency baseCurrency)
    {
        var response = new StringBuilder();

        try
        {
            if (baseCurrency.BaseCurrencyLit == string.Empty || baseCurrency.Date < DateTime.Today.AddYears(-4) ||
                baseCurrency.Date > DateTime.Today)
            {
                throw new FormatException("Wrong BaseCurrency format.");
            }

            if (baseCurrency.ExchangeRate.Count == 0)
            {
                return Task.FromResult("There are no available rates at the moment. Try another date or try again later");
            }

            if (baseCurrency.Date == DateTime.Today)
            {
                response.Append($"Today`s {baseCurrency.BaseCurrencyLit} exchange rates are:");
            }
            else
            {
                response.Append($"The {baseCurrency.BaseCurrencyLit} exchange rates on the this date({baseCurrency.Date:dd.MM.yyyy}) were:");
            }

            foreach (var currency in baseCurrency.ExchangeRate)
            {
                if (currency.PurchaseRate > 0 && currency.SaleRate > 0)
                {
                    response.Append($"\n{currency.Currency} - {currency.PurchaseRate}/{currency.SaleRate}");
                }
            }
        }
        catch (Exception e)
        {
            response.Append("Something went wrong. I will fix it as soon as possible. Come back later.");
            Console.WriteLine($"ExchangeRateService.GetAllRates: {e.Message}");
        }

        return Task.FromResult(response.ToString());
    }

    public Task<string> GetSelectedRates(BaseCurrency baseCurrency, string currencyLit)
    {
        var response = new StringBuilder();

        try
        {
            if (baseCurrency.BaseCurrencyLit == string.Empty || baseCurrency.Date < DateTime.Today.AddYears(-4) ||
                baseCurrency.Date > DateTime.Today)
            {
                throw new FormatException("Wrong BaseCurrency format.");
            }

            if (baseCurrency.ExchangeRate.Count == 0)
            {
                return Task.FromResult("There are no available rates at the moment. Try another date or try again later");
            }

            if (baseCurrency.Date == DateTime.Today)
            {
                response.Append($"Today`s {baseCurrency.BaseCurrencyLit} to {currencyLit} exchange rate is: ");
            }
            else
            {
                response.Append($"The {baseCurrency.BaseCurrencyLit} to {currencyLit} exchange rate on the this date({baseCurrency.Date:dd.MM.yyyy}) was: ");
            }

            if (!baseCurrency.ExchangeRate.Exists(c => c.Currency == currencyLit))
            {
                return Task.FromResult("I can not find needed rate. Try another date or try again later");
            }

            var minorCurrency = baseCurrency.ExchangeRate.First(
                c => c.Currency == currencyLit);

            if (minorCurrency.PurchaseRate == 0 || minorCurrency.SaleRate == 0)
            {
                return Task.FromResult("No rate for this day. Try another date or try again later");
            }

            response.Append($"{minorCurrency.PurchaseRate}/{minorCurrency.SaleRate}");
        }
        catch (Exception e)
        {
            response.Append("Something went wrong. I will fix it as soon as possible. Come back later.");
            Console.WriteLine($"ExchangeRateService.GetSelectedRates: {e.Message}");
        }

        return Task.FromResult(response.ToString());
    }

    public async Task<BaseCurrency> GetBaseCurrency(DateTime date)
    {
        if (date < DateTime.Today.AddYears(-4) || date > DateTime.Today)
        {
            throw new FormatException("ExchangeRateService.GetBaseCurrency: Wrong date format.");
        }

        try
        {
            var url = await new ConfigurationRequest().GetApiUrl(date, ApiKey);
            var jsonResponse = await new ApiJsonRequest().GetStringResponse(url);
            var baseCurrency = JsonConvert.DeserializeObject<BaseCurrency> (jsonResponse, new IsoDateTimeConverter{DateTimeFormat = "dd.MM.yyyy"});

            if (baseCurrency == null || baseCurrency.BaseCurrencyLit == string.Empty || baseCurrency.Date < DateTime.Today.AddYears(-4) ||
                baseCurrency.Date > DateTime.Today)
            {
                throw new FormatException("Wrong BaseCurrency format.");
            }

            return baseCurrency;
        }
        catch (Exception e)
        {
            throw new Exception($"ExchangeRateService.GetBaseCurrency: {e.Message}");
        }
    }

    public  Task<List<string>> GetAvailableRates(BaseCurrency baseCurrency)
    {
        var rates = new List<string>();

        foreach (var currency in baseCurrency.ExchangeRate)
        {
            if (currency.PurchaseRate > 0 && currency.SaleRate > 0)
            {
                rates.Add(currency.Currency);
            }
        }

        return Task.FromResult(rates);
    }
}