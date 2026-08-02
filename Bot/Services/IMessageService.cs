namespace Bot.Services;

public interface IMessageService
{
    Task SendMessage(long chatId, ITelegramBotClient client, string text,
        InlineKeyboardMarkup? inlineKeyboardMarkup);

    Task SendMessage(long chatId, ITelegramBotClient client, string caption, string? path,
        InlineKeyboardMarkup? inlineKeyboardMarkup);

    Task EditMessage(long chatId, int messageId, ITelegramBotClient client,
        string text, InlineKeyboardMarkup? inlineKeyboardMarkup);

    Task EditMediaMessage(long chatId, int messageId, ITelegramBotClient client,
        string? caption, string path, InlineKeyboardMarkup? inlineKeyboardMarkup);

    Task DeleteMessage(long chatId, int messageId, ITelegramBotClient client);

    Task ShowAlert(string callbackQueryId, ITelegramBotClient client, string message);

    Task<string> GetMessageText(string name, string language = "ru");

    Task<string?> GetMessagePathToPhoto(string name);

    string Escape(string? text);
}
