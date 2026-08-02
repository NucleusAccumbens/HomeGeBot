using Application.Messages.Queries.GetMessageBody;
using Application.Users.Queries.GetUserLanguage;
using MediatR;

namespace Bot.Services;

public class LocalizedMessageResolver : ILocalizedMessageResolver
{
    private readonly IMediator _mediator;

    public LocalizedMessageResolver(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<string> ResolveAsync(long chatId, string messageName, string fallback, CancellationToken cancellationToken = default)
    {
        var lang = await _mediator.Send(new GetUserLanguageQuery(new Domain.Common.ChatId(chatId)), cancellationToken);
        return await _mediator.Send(new GetMessageBodyQuery(messageName, lang), cancellationToken) ?? fallback;
    }
}
