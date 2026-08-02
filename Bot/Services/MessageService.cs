using Application.Messages.Queries.GetMessageBody;
using Application.Messages.Queries.GetMessagePathToPhoto;
using MediatR;

namespace Bot.Services;

public static class MessageService
{
    public static Task SendMessage(long chatId, ITelegramBotClient client, string text,
        InlineKeyboardMarkup? inlineKeyboardMarkup)
    {
        return client.SendTextMessageAsync(
            chatId: chatId,
            text: text,
            parseMode: ParseMode.Html,
            disableWebPagePreview: true,
            replyMarkup: inlineKeyboardMarkup);
    }

    public static Task SendMessage(long chatId, ITelegramBotClient client, string caption, string? path,
        InlineKeyboardMarkup? inlineKeyboardMarkup)
    {
        if (path != null)
        {
            return client.SendPhotoAsync(
                chatId: chatId,
                photo: path,
                caption: caption,
                parseMode: ParseMode.Html,
                replyMarkup: inlineKeyboardMarkup);
        }

        return Task.CompletedTask;
    }

    public static Task EditMessage(long chatId, int messageId, ITelegramBotClient client,
        string text, InlineKeyboardMarkup? inlineKeyboardMarkup)
    {
        return client.EditMessageTextAsync(
            chatId: chatId,
            messageId: messageId,
            text: text,
            parseMode: ParseMode.Html,
            disableWebPagePreview: true,
            replyMarkup: inlineKeyboardMarkup);
    }

    public static Task EditMediaMessage(long chatId, int messageId, ITelegramBotClient client,
        string? captcha, string path, InlineKeyboardMarkup? inlineKeyboardMarkup)
    {
        var media = new InputMediaPhoto(new InputMedia(path));

        media.Caption = captcha;

        media.ParseMode = ParseMode.Html;

        return client.EditMessageMediaAsync(
            chatId: chatId,
            messageId: messageId,
            media: media,
            replyMarkup: inlineKeyboardMarkup);
    }

    public static Task DeleteMessage(long chatId, int messageId, ITelegramBotClient client)
    {
        return client.DeleteMessageAsync(
            chatId: chatId,
            messageId: messageId);
    }

    public static Task ShowAllert(string callbackQueryId, ITelegramBotClient client, string message)
    {
        return client.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQueryId,
                text: message,
                showAlert: true);
    }

    public static async Task<string> GetMessageText(IMediator mediator, string name, string language = "ru")
    {
        var messageText = await mediator.Send(new GetMessageBodyQuery(name, language));

        if (!string.IsNullOrEmpty(messageText)) return messageText;

        else return "Сообщение с таким именем не найдено";
    }

    public static Task<string?> GetMessagePathToPhoto(IMediator mediator, string name)
    {
        return mediator.Send(new GetMessagePathToPhotoQuery(name));
    }

    public static string Escape(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return System.Net.WebUtility.HtmlEncode(text);
    }
}
