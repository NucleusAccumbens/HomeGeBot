using Application.TlgUsers.Queries.CheckUserStatus;
using Bot.Common.Interfaces;
using MediatR;

namespace Bot.Services;

public class UserStatusChecker : IUserStatusChecker
{
    private readonly IMediator _mediator;

    public UserStatusChecker(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<bool> IsKickedAsync(long? chatId, CancellationToken cancellationToken = default)
    {
        if (chatId == null) return false;
        return await _mediator.Send(new CheckUserStatusQuery(chatId.Value), cancellationToken);
    }
}
