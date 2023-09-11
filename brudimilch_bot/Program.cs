
using brudimilch_bot;
using brudimilch_bot.Entities;
using System.Text.RegularExpressions;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using var db = new ReminderContext();
var botClient = new TelegramBotClient(AppSettings.BotToken);
var pattern = @"^-?[0-9]+(?:\.[0-9]+)?$";
var regex = new Regex(pattern);
Console.WriteLine($"Database path: {db.DbPath}");

using CancellationTokenSource cts = new();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

var timer = new System.Timers.Timer();
timer.Interval = 1000;
timer.Elapsed += Timer_Elapsed;
timer.AutoReset = true;
timer.Start();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
{
    using var getdb = new ReminderContext();
    await getdb.Database.EnsureCreatedAsync();
    var reminders = getdb.Reminders.Where(x => x.Date <= DateTime.Now && !x.ReminderSent).ToArray();
    foreach (var reminder in reminders)
    {
        Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: reminder.ChatId,
                    text: $"Reminder" + (!string.IsNullOrWhiteSpace(reminder.Name) ? $" for {reminder.Name}" : string.Empty) +$". {reminder.Date}",
                    cancellationToken: new CancellationToken());

        reminder.ReminderSent = true;
        await getdb.SaveChangesAsync();
    }
}

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

    // Echo received message text


    if (messageText.StartsWith("/createreminder", StringComparison.InvariantCultureIgnoreCase))
    {
        var onlyNumbers = true;

        var datePart = messageText.Split(' ')[1].Split('.');
        var timePart = messageText.Split(' ')[2].Split(':');

        datePart.ToList().ForEach(x => { if (!regex.IsMatch(x)) onlyNumbers = false; });
        timePart.ToList().ForEach(x => { if (!regex.IsMatch(x)) onlyNumbers = false; });

        if (onlyNumbers)
        {
            try
            {
                var convertedDates = datePart.Select(x => int.Parse(x)).ToArray();
                var convertedTimes = timePart.Select(x => int.Parse(x)).ToArray();
                var reminder = new Reminder
                {
                    Date = new DateTime(convertedDates[0], convertedDates[1], convertedDates[2], convertedTimes[0], convertedTimes[1], convertedTimes[2]),
                    Name = messageText.Split(' ').Length > 3 ? string.Join(' ', messageText.Split(' ').Skip(3)) : "",
                    ChatId = chatId,
                    ReminderSent = false,
                };
                await db.Database.EnsureCreatedAsync();
                await db.AddAsync(reminder);
                await db.SaveChangesAsync();
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Reminder created successfully for {reminder.Date}!",
                    cancellationToken: cancellationToken);
            }
            catch
            {
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "You said:\n" + messageText + "\nCould not parse date. Please send Date in the following format: YYYY.MM.DD HH:mm:SS",
                    cancellationToken: cancellationToken);
            }
        }
        else
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "You said:\n" + messageText + "\nCould not parse date. Please send Date in the following format: YYYY.MM.DD HH:mm:SS",
                cancellationToken: cancellationToken);
        }
    }
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}
