using Telegram.Bot;
using Telegram.Bot.Types;

namespace CurrencyTelegramBot.Services.Interfaces;

public interface ICurrencyBotService
{
    Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken);
    Task HandleMessage(ITelegramBotClient client, Message message);
    Task HandleCallbackQuery(ITelegramBotClient client, CallbackQuery callbackQuery);
    Task HandlePollingErrorAsync(ITelegramBotClient client, Exception update, CancellationToken cancellationToken);
}