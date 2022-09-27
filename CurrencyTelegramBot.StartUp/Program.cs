using CurrencyTelegramBot.Services.Implementations;
using Telegram.Bot;
using CurrencyTelegramBot.Services.Requests;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

var configuration = new ConfigurationRequest();

var botClient = new TelegramBotClient(await configuration.GetValue( "TelegramBotToken"));

var botService = new CurrencyBotService();

using var cts = new CancellationTokenSource();

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>()
};

botClient.StartReceiving(
    updateHandler: botService.HandleUpdateAsync,
    pollingErrorHandler: botService.HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

cts.Cancel();