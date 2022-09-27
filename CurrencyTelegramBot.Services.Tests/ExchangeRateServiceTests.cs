using CurrencyTelegramBot.Models.Models;
using CurrencyTelegramBot.Services.Implementations;
using NUnit.Framework;

namespace CurrencyTelegramBot.Services.Tests;

public class ExchangeRateServiceTests
{
    private ExchangeRateService _rateService;

    [SetUp]
    public void Setup()
    {
        _rateService = new ExchangeRateService();
    }

    [Test]
    public async Task GetBaseCurrency_CorrectData_AreEqual()
    {
        var expected = new BaseCurrency {BaseCurrencyLit = "UAH", Date = DateTime.Today};

        var actual = await _rateService.GetBaseCurrency(DateTime.Today);

        Assert.Multiple(() =>
        {
            Assert.That(actual.BaseCurrencyLit, Is.EqualTo(expected.BaseCurrencyLit));
            Assert.That(actual.Date, Is.EqualTo(expected.Date));
        });
    }

    [Test]
    public void GetBaseCurrency_InvalidData_Exception()
    {
        Assert.Multiple(() =>
        {
            Assert.ThrowsAsync<FormatException>(() => _ = _rateService.GetBaseCurrency(new DateTime(2000, 01, 01)));
            Assert.ThrowsAsync<FormatException>(() => _ = _rateService.GetBaseCurrency(new DateTime(3000, 01, 01)));
        });
    }

    [Test]
    public async Task GetAvailableRates_CorrectData_AreEqual()
    {
        var testBaseCurrency = new BaseCurrency {BaseCurrencyLit = "UAH", Date = DateTime.Today, ExchangeRate = new List<MinorCurrency>{
            new MinorCurrency{BaseCurrency = "UAH", Currency = "USD", PurchaseRate = 42, SaleRate = 43}, 
            new MinorCurrency{BaseCurrency = "UAH", Currency = "EUR", PurchaseRate = 44, SaleRate = 45}}};

        var expectedAvailableRates = new List<string>{"USD", "EUR"};

        var actualAvailableRates = await _rateService.GetAvailableRates(testBaseCurrency);

        Assert.Multiple(() =>
        {
            Assert.That(actualAvailableRates, Is.EqualTo(expectedAvailableRates));
        });
    }

    [Test]
    public async Task GetAllRates_CorrectData_AreEqual()
    {
        var testBaseCurrency = new BaseCurrency
        {
            BaseCurrencyLit = "UAH",
            Date = DateTime.Today,
            ExchangeRate = new List<MinorCurrency>{
                new MinorCurrency{BaseCurrency = "UAH", Currency = "USD", PurchaseRate = 42, SaleRate = 43},
                new MinorCurrency{BaseCurrency = "UAH", Currency = "EUR", PurchaseRate = 44, SaleRate = 45}}
        };

        const string expected = "Today`s UAH exchange rates are:\nUSD - 42/43\nEUR - 44/45";

        var actual = await _rateService.GetAllRates(testBaseCurrency);

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(expected));
        });
    }

    [Test]
    public async Task GetAllRates_InvalidData_Response()
    {
        var testBaseCurrency1 = new BaseCurrency()
        {
            BaseCurrencyLit = "UAH",
            Date = new DateTime(2000, 01, 01),
        };
        var testBaseCurrency2 = new BaseCurrency()
        {
            BaseCurrencyLit = "UAH",
            Date = new DateTime(3000, 01, 01),
        };
        var testBaseCurrency3 = new BaseCurrency()
        {
            BaseCurrencyLit = "UAH",
            Date = DateTime.Today,
        };

        const string expectedResponse1 = "Something went wrong. I will fix it as soon as possible. Come back later.";
        const string expectedResponse2 = "There are no available rates at the moment. Try another date or try again later";

        var actual1 = await _rateService.GetAllRates(testBaseCurrency1);
        var actual2= await _rateService.GetAllRates(testBaseCurrency2);
        var actual3 = await _rateService.GetAllRates(testBaseCurrency3);

        Assert.Multiple(() =>
        {
            Assert.That(actual1, Is.EqualTo(expectedResponse1));
            Assert.That(actual2, Is.EqualTo(expectedResponse1));
            Assert.That(actual3, Is.EqualTo(expectedResponse2));
        });
    }

    [Test]
    public async Task GetSelectedRates_CorrectData_AreEqual()
    {
        var testBaseCurrency = new BaseCurrency
        {
            BaseCurrencyLit = "UAH",
            Date = DateTime.Today,
            ExchangeRate = new List<MinorCurrency>{
                new MinorCurrency{BaseCurrency = "UAH", Currency = "USD", PurchaseRate = 42, SaleRate = 43},
                new MinorCurrency{BaseCurrency = "UAH", Currency = "EUR", PurchaseRate = 44, SaleRate = 45}}
        };

        const string expected = "Today`s UAH to USD exchange rate is: 42/43";

        var actual = await _rateService.GetSelectedRates(testBaseCurrency, "USD");

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(expected));
        });
    }

    [Test]
    public async Task GetSelectedRates_InvalidData_Response()
    {
        var testBaseCurrency1 = new BaseCurrency
        {
            BaseCurrencyLit = "UAH",
            Date = new DateTime(2000, 01, 01),
        };
        var testBaseCurrency2 = new BaseCurrency
        {
            BaseCurrencyLit = "UAH",
            Date = new DateTime(3000, 01, 01),
        };
        var testBaseCurrency3 = new BaseCurrency
        {
            BaseCurrencyLit = "UAH",
            Date = DateTime.Today,
        };
        var testBaseCurrency4 = new BaseCurrency
        {
            BaseCurrencyLit = "UAH",
            Date = DateTime.Today,
            ExchangeRate = new List<MinorCurrency>{
                new MinorCurrency{BaseCurrency = "UAH", Currency = "USD", PurchaseRate = 42, SaleRate = 43},
                new MinorCurrency{BaseCurrency = "UAH", Currency = "EUR", PurchaseRate = 44, SaleRate = 45}}
        };
        var testBaseCurrency5 = new BaseCurrency
        {
            BaseCurrencyLit = "UAH",
            Date = DateTime.Today,
            ExchangeRate = new List<MinorCurrency>{
                new MinorCurrency{BaseCurrency = "UAH", Currency = "USD", PurchaseRate = 0, SaleRate = 0},
                new MinorCurrency{BaseCurrency = "UAH", Currency = "EUR", PurchaseRate = 44, SaleRate = 45}}
        };

        const string expectedResponse1 = "Something went wrong. I will fix it as soon as possible. Come back later.";
        const string expectedResponse2 = "There are no available rates at the moment. Try another date or try again later";
        const string expectedResponse3 = "I can not find needed rate. Try another date or try again later";
        const string expectedResponse4 = "No rate for this day. Try another date or try again later";

        var actual1 = await _rateService.GetSelectedRates(testBaseCurrency1, "USD");
        var actual2 = await _rateService.GetSelectedRates(testBaseCurrency2, "USD");
        var actual3 = await _rateService.GetSelectedRates(testBaseCurrency3, "USD");
        var actual4 = await _rateService.GetSelectedRates(testBaseCurrency4, "RUR");
        var actual5 = await _rateService.GetSelectedRates(testBaseCurrency5, "USD");

        Assert.Multiple(() =>
        {
            Assert.That(actual1, Is.EqualTo(expectedResponse1));
            Assert.That(actual2, Is.EqualTo(expectedResponse1));
            Assert.That(actual3, Is.EqualTo(expectedResponse2));
            Assert.That(actual4, Is.EqualTo(expectedResponse3));
            Assert.That(actual5, Is.EqualTo(expectedResponse4));
        });
    }
}