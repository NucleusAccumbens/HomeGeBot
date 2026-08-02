using Bot.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot.UpdateHandlers;

public class MessageUpdateHandler : IUpdateHandler
{
    private readonly IUserStatusChecker _kickChecker;
    private readonly ITextCommandRouter _textRouter;
    private readonly ILogger<MessageUpdateHandler> _logger;

    public MessageUpdateHandler(IUserStatusChecker kickChecker,
        ITextCommandRouter textRouter,
        ILogger<MessageUpdateHandler> logger)
    {
        _kickChecker = kickChecker;
        _textRouter = textRouter;
        _logger = logger;
    }

    public bool CanHandle(UpdateType type) => type == UpdateType.Message;

    public async Task HandleAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null || !IsTextPhotoOrVideo(update.Message))
            return;

        var chatId = update.Message.Chat.Id;

        if (await _kickChecker.IsKickedAsync(chatId, cancellationToken))
            return;

        await _textRouter.RouteAsync(client, update, chatId, cancellationToken);
    }

    private static bool IsTextPhotoOrVideo(Message? message)
    {
        return message != null &&
               (message.Type == MessageType.Text ||
                message.Type == MessageType.Photo ||
                message.Type == MessageType.Video);
    }
}
