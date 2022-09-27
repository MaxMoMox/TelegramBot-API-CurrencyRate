using System.Globalization;
using CurrencyTelegramBot.Models.Models;
using CurrencyTelegramBot.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CurrencyTelegramBot.Services.Implementations;

public class CurrencyBotService : ICurrencyBotService
{
    private BaseCurrency _baseCurrency;
    private readonly ExchangeRateService _exchangeRateService;

    private readonly ReplyKeyboardMarkup _replyKeyboardMarkup = 
        new
        (new[]
        {
            new KeyboardButton[] { "Today`s rates" },
            new KeyboardButton[]{ "USD today", "EUR today" }
        })
        { ResizeKeyboard = true };

    private readonly string[] _inputFormats = { "dd.MM.yyyy", "d.M.yyyy", "d.M.yy", "dd/MM/yyyy", "d/M/yyyy", "d/M/yy" };

    public CurrencyBotService(BaseCurrency baseCurrency, ExchangeRateService exchangeRateService)
    {
        _baseCurrency = baseCurrency;
        _exchangeRateService = exchangeRateService;
    }

    public CurrencyBotService() : this(new BaseCurrency(), new ExchangeRateService())
    {
    }

    public async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            await HandleMessage(client, update.Message);
            return;
        }

        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
        {
            await HandleCallbackQuery(client, update.CallbackQuery);
        }
    }

    public async Task HandleMessage(ITelegramBotClient client, Message message)
    {
        if (message.Text != null)
        {
            if (message.Text == "Today`s rates")
            {
                if (_baseCurrency.Date != DateTime.Today)
                {
                    _baseCurrency = await _exchangeRateService.GetBaseCurrency(DateTime.Today);
                }

                await client.SendTextMessageAsync(message.Chat.Id, await _exchangeRateService.GetAllRates(_baseCurrency),
                    replyToMessageId: message.MessageId, replyMarkup: _replyKeyboardMarkup);
            }
            else if (message.Text == "USD today")
            {
                if (_baseCurrency.Date != DateTime.Today)
                {
                    _baseCurrency = await _exchangeRateService.GetBaseCurrency(DateTime.Today);
                }

                await client.SendTextMessageAsync(message.Chat.Id,
                    await _exchangeRateService.GetSelectedRates(_baseCurrency, "USD"),
                    replyToMessageId: message.MessageId, replyMarkup: _replyKeyboardMarkup);
            }
            else if (message.Text == "EUR today")
            {
                if (_baseCurrency.Date != DateTime.Today)
                {
                    _baseCurrency = await _exchangeRateService.GetBaseCurrency(DateTime.Today);
                }

                await client.SendTextMessageAsync(message.Chat.Id,
                    await _exchangeRateService.GetSelectedRates(_baseCurrency, "EUR"),
                    replyToMessageId: message.MessageId, replyMarkup: _replyKeyboardMarkup);
            }
            else if (!DateTime.TryParseExact(message.Text, _inputFormats, CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var date))
            {
                await client.SendTextMessageAsync(message.Chat.Id,
                    "Wrong date format! Please, write date in 'dd.mm.yyyy'(01.01.2022) or 'dd/mm/yyyy'(01/01/2022) format.",
                    replyToMessageId: message.MessageId, replyMarkup: _replyKeyboardMarkup);
            }
            else if (date > DateTime.Today)
            {
                await client.SendTextMessageAsync(message.Chat.Id,
                    "I can not see the future! Try again.", replyToMessageId: message.MessageId,
                    replyMarkup: _replyKeyboardMarkup);
            }
            else if (date < DateTime.Today.AddYears(-4))
            {
                await client.SendTextMessageAsync(message.Chat.Id,
                    "It was too long time ago. I only remember the exchange rate for the last 4 years. Try again.",
                    replyToMessageId: message.MessageId, replyMarkup: _replyKeyboardMarkup);
            }
            else
            {
                if (_baseCurrency.Date != date)
                {
                    _baseCurrency = await _exchangeRateService.GetBaseCurrency(date);
                }

                var availableRates = await _exchangeRateService.GetAvailableRates(_baseCurrency);

                if (availableRates.Count == 0)
                {
                    await client.SendTextMessageAsync(message.Chat.Id,
                        "There are no available rates at the moment. Try another date or try again later.",
                        replyToMessageId: message.MessageId);

                    await Task.CompletedTask;
                }

                var buttons = new List<InlineKeyboardButton[]>();

                for (var i = 0; i < availableRates.Count; i++)
                {
                    if (availableRates.Count - 1 == i)
                    {
                        buttons.Add(new[] { new InlineKeyboardButton(availableRates[i]) { CallbackData = availableRates[i] } });
                    }
                    else
                    {
                        buttons.Add(new[]
                        {
                        new InlineKeyboardButton(availableRates[i]) {CallbackData = availableRates[i]},
                        new InlineKeyboardButton(availableRates[i + 1])
                        {
                            CallbackData = availableRates[i + 1]

                        }});
                    }

                    i++;
                }

                buttons.Add(new[] { new InlineKeyboardButton("Show all") { CallbackData = "all" } });

                var keyboard = new InlineKeyboardMarkup(buttons.ToArray());

                await client.SendTextMessageAsync(message.Chat.Id,
                    "Please, select the currency:", replyToMessageId: message.MessageId, replyMarkup: keyboard);
            }
        }
    }

    public async Task HandleCallbackQuery(ITelegramBotClient client, CallbackQuery callbackQuery)
    {
        if (callbackQuery.Data != null && callbackQuery.Message != null && _baseCurrency.ExchangeRate.Count != 0)
        {
            if (_baseCurrency.ExchangeRate.Count == 0)
            {
                await client.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id, "There are no available rates at the moment. Try to write the date again.");
            }
            else if (_baseCurrency.ExchangeRate.Any(c => c.Currency == callbackQuery.Data))
            {
                await client.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id, await _exchangeRateService.GetSelectedRates(_baseCurrency, callbackQuery.Data));
            }
            else if (callbackQuery.Data == "all")
            {
                await client.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id, await _exchangeRateService.GetAllRates(_baseCurrency));
            }
            else
            {
                await client.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id, "I can not find it. Try to write the date again.");
            }
        }
        else if (callbackQuery.Message != null)
        {
            Console.WriteLine("CurrencyBotService.HandleCallbackQuery: CallbackQuery is null.");

            await client.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id, "Something went wrong.I will fix it as soon as possible. Come back later.");
        }
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);

        return Task.CompletedTask;
    }
}