using Application.Messages.Interfaces;
using Bot.Common.Services;

namespace Bot.Common.Abstractions;

public abstract class BaseMessage
{
    private readonly string _messageName;

    private readonly IGetMessageQuery _getMessageQuery;

    public BaseMessage(string messageName, IGetMessageQuery getMessageQuery)
    {
        _messageName = messageName;
        _getMessageQuery = getMessageQuery;
    }

    public virtual InlineKeyboardMarkup? InlineKeyboardMarkup { get; }

    public virtual async Task SendMessage(long chatId, ITelegramBotClient client)
    {
        await MessageService
            .SendMessage(chatId, client, await GetMessageBody(), InlineKeyboardMarkup);
    }

    public virtual async Task SendPhoto(long chatId, ITelegramBotClient client)
    {
        await MessageService
            .SendMessage(chatId, client, await GetMessageBody(),
            await GetMessagePathToPhoto(), InlineKeyboardMarkup);
    }

    public virtual async Task SendWelcomeMessage(long chatId, ITelegramBotClient client, string username)
    {
        string messageBody = $"Привет, <b>{username}</b>!\n\n" + await GetMessageBody();


        await MessageService
            .SendMessage(chatId, client, messageBody, InlineKeyboardMarkup);
    }

    public virtual async Task EditMessage(long chatId, int messageId, ITelegramBotClient client)
    {
        await MessageService
            .EditMessage(chatId, messageId, client, await GetMessageBody(), InlineKeyboardMarkup);
    }

    public virtual async Task EditMessage(long chatId, int messageId, ITelegramBotClient client, string message)
    {
        string messageBody = await GetMessageBody();

        await MessageService
            .EditMessage(chatId, messageId, client, $"{message}\n\n{messageBody}", InlineKeyboardMarkup);
    }

    public virtual async Task EditMediaMessage(long chatId, int messageId, ITelegramBotClient client)
    {
        await MessageService
            .EditMediaMessage(chatId, messageId, client, await GetMessageBody(),
            await GetMessagePathToPhoto(), InlineKeyboardMarkup);
    }

    private async Task<string> GetMessageBody()
    {
        return await MessageService.GetMessageText(_getMessageQuery, _messageName);
    }

    private async Task<string> GetMessagePathToPhoto()
    {
        return await MessageService.GetMessagePathToPhoto(_getMessageQuery, _messageName);
    }
}
