using Bot.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot.UpdateHandlers;

public class CallbackQueryUpdateHandler : IUpdateHandler
{
    private readonly IUserStatusChecker _kickChecker;
    private readonly ICallbackCommandRouter _callbackRouter;
    private readonly ILogger<CallbackQueryUpdateHandler> _logger;

    public CallbackQueryUpdateHandler(IUserStatusChecker kickChecker,
        ICallbackCommandRouter callbackRouter,
        ILogger<CallbackQueryUpdateHandler> logger)
    {
        _kickChecker = kickChecker;
        _callbackRouter = callbackRouter;
        _logger = logger;
    }

    public bool CanHandle(UpdateType type) => type == UpdateType.CallbackQuery;

    public async Task HandleAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery?.Message == null) return;

        if (await _kickChecker.IsKickedAsync(update.CallbackQuery.Message.Chat.Id, cancellationToken))
            return;

        await _callbackRouter.RouteAsync(client, update, cancellationToken);
        await client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, cancellationToken: cancellationToken);
    }
}
