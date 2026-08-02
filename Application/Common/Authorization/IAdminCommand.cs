using Domain.Common;

namespace Application.Common.Authorization;

public interface IAdminCommand
{
    ChatId AdminChatId { get; }
}
