using Application.Messages.Queries.GetMessageBody;
using Application.Messages.Queries.GetMessagePathToPhoto;
using MediatR;

namespace Bot.Services;

public class MessageService : IMessageService
{
    private readonly IMediator _mediator;

    public MessageService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task SendMessage(long chatId, ITelegramBotClient client, string text,
        InlineKeyboardMarkup? inlineKeyboardMarkup)
    {
        return client.SendTextMessageAsync(
            chatId: chatId,
            text: text,
            parseMode: ParseMode.Html,
            disableWebPagePreview: true,
            replyMarkup: inlineKeyboardMarkup);
    }

    public Task SendMessage(long chatId, ITelegramBotClient client, string caption, string? path,
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

    public Task EditMessage(long chatId, int messageId, ITelegramBotClient client,
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

    public Task EditMediaMessage(long chatId, int messageId, ITelegramBotClient client,
        string? caption, string path, InlineKeyboardMarkup? inlineKeyboardMarkup)
    {
        var media = new InputMediaPhoto(new InputMedia(path));

        media.Caption = caption;

        media.ParseMode = ParseMode.Html;

        return client.EditMessageMediaAsync(
            chatId: chatId,
            messageId: messageId,
            media: media,
            replyMarkup: inlineKeyboardMarkup);
    }

    public Task DeleteMessage(long chatId, int messageId, ITelegramBotClient client)
    {
        return client.DeleteMessageAsync(
            chatId: chatId,
            messageId: messageId);
    }

    public Task ShowAlert(string callbackQueryId, ITelegramBotClient client, string message)
    {
        return client.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQueryId,
                text: message,
                showAlert: true);
    }

    public async Task<string> GetMessageText(string name, string language = "ru")
    {
        var messageText = await _mediator.Send(new GetMessageBodyQuery(name, language));

        if (!string.IsNullOrEmpty(messageText)) return messageText;

        else return "Сообщение с таким именем не найдено";
    }

    public Task<string?> GetMessagePathToPhoto(string name)
    {
        return _mediator.Send(new GetMessagePathToPhotoQuery(name));
    }

    public string Escape(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return System.Net.WebUtility.HtmlEncode(text);
    }
}
