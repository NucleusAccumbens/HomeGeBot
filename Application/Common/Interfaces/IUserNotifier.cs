using Domain.Common;

namespace Application.Common.Interfaces;

public interface IUserNotifier
{
    Task SendNotificationAsync(ChatId chatId, string message);
}
