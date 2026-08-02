using Application.TlgUsers.Commands.ToggleUserKick;
using Bot.Common.Interfaces;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot.UpdateHandlers;

public class MyChatMemberUpdateHandler : IUpdateHandler
{
    private readonly IMediator _mediator;

    public MyChatMemberUpdateHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public bool CanHandle(UpdateType type) => type == UpdateType.MyChatMember;

    public async Task HandleAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (update.MyChatMember != null)
        {
            await _mediator.Send(new ToggleUserKickCommand(update.MyChatMember.Chat.Id), cancellationToken);
        }
    }
}
