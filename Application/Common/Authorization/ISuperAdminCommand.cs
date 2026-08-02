using Domain.Common;

namespace Application.Common.Authorization;

public interface ISuperAdminCommand
{
    ChatId SuperAdminChatId { get; }
}
