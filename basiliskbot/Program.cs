using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

string[] phrases = File.ReadAllLines("dataset.txt");
string accessToken = Environment.GetEnvironmentVariable("BASILISKBOT_TELEGRAM_TOKEN")!;

if (accessToken == null)
{
    throw new Exception("BASILISKBOT_TELEGRAM_TOKEN environment variable is not defined");
}

ITelegramBotClient botClient = new TelegramBotClient(accessToken);
var random = new Random();

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = new UpdateType[] { UpdateType.Message },
    ThrowPendingUpdates = true
};

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions);

User botAccount = await botClient.GetMeAsync();
Console.WriteLine($"Start listening druzhokbot for @{botAccount.Username}");

await Task.Delay(Timeout.Infinite);

async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken _)
{
    try
    {
        Message message = update.Message!;
        Chat chat = message.Chat;

        // Start command
        if (message.Text == "/start")
        {
            await bot.SendTextMessageAsync(chat.Id, "Привіт, додай мене до свого чату і я буду хейтити войси.", replyToMessageId: message.MessageId);
            return;
        }

        // Ignore all except voice
        if (message.Voice == null)
        {
            return;
        }

        string phrase = phrases[random.Next(phrases.Length)];

        await bot.SendTextMessageAsync(chat.Id, phrase, replyToMessageId: message.MessageId);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
}

async Task HandleErrorAsync(ITelegramBotClient bot, Exception error, CancellationToken _) =>
    Console.WriteLine(error);
