using Telegram.Bot.Types;

namespace Bot.Common.Interfaces;

public interface IUpdateHandler
{
    bool CanHandle(UpdateType type);
    Task HandleAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken);
}
