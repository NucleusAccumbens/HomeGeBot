using Bot.Services;
using MediatR;
using Application.Users.Queries.GetUserLanguage;

namespace Bot.Common.Abstractions;

public abstract class BaseMessage
{
    private readonly string _messageName;
    private readonly IMediator _mediator;
    private readonly IMessageService _messageService;

    public BaseMessage(string messageName, IMediator mediator, IMessageService messageService)
    {
        _messageName = messageName;
        _mediator = mediator;
        _messageService = messageService;
    }

    public virtual InlineKeyboardMarkup? GetInlineKeyboardMarkup(string language) => null;

    public virtual async Task SendMessage(long chatId, ITelegramBotClient client)
    {
        var language = await _mediator.Send(new GetUserLanguageQuery(chatId));
        await _messageService
            .SendMessage(chatId, client, await GetMessageBody(chatId), GetInlineKeyboardMarkup(language));
    }

    public virtual async Task SendPhoto(long chatId, ITelegramBotClient client)
    {
        var language = await _mediator.Send(new GetUserLanguageQuery(chatId));
        await _messageService
            .SendMessage(chatId, client, await GetMessageBody(chatId),
            await GetMessagePathToPhoto(), GetInlineKeyboardMarkup(language));
    }

    public virtual async Task EditMessage(long chatId, int messageId, ITelegramBotClient client)
    {
        var language = await _mediator.Send(new GetUserLanguageQuery(chatId));
        await _messageService
            .EditMessage(chatId, messageId, client, await GetMessageBody(chatId), GetInlineKeyboardMarkup(language));
    }

    public virtual async Task EditMessage(long chatId, int messageId, ITelegramBotClient client, string message)
    {
        var language = await _mediator.Send(new GetUserLanguageQuery(chatId));
        string messageBody = await GetMessageBody(chatId);

        await _messageService
            .EditMessage(chatId, messageId, client, $"{message}\n\n{messageBody}", GetInlineKeyboardMarkup(language));
    }

    private async Task<string> GetMessageBody(long chatId)
    {
        var language = await _mediator.Send(new GetUserLanguageQuery(chatId));
        return await _messageService.GetMessageText(_messageName, language);
    }

    private async Task<string?> GetMessagePathToPhoto()
    {
        return await _messageService.GetMessagePathToPhoto(_messageName);
    }
}
