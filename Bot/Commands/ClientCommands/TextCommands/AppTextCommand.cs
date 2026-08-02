using Bot.Common.Abstractions;
using Bot.Configuration;
using Bot.Exceptions;
using Bot.Services;
using Bot.Session;
using FluentValidation;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace Bot.Commands.ClientCommands.TextCommands;

public class AppTextCommand : BaseTextCommand
{
    private readonly IBotSessionStore _sessionStore;
    private readonly IMessageService _messageService;
    private readonly ILocalizedMessageResolver _messageResolver;
    private readonly IRentalApplicationForwardProcessor _forwardProcessor;
    private readonly BotConfiguration _botConfig;

    public AppTextCommand(IBotSessionStore sessionStore,
        IMessageService messageService,
        ILocalizedMessageResolver messageResolver,
        IRentalApplicationForwardProcessor forwardProcessor,
        IOptions<BotConfiguration> botConfigOptions)
    {
        _sessionStore = sessionStore;
        _messageService = messageService;
        _messageResolver = messageResolver;
        _forwardProcessor = forwardProcessor;
        _botConfig = botConfigOptions.Value;
    }

    public override string Name => "app";

    public override BotStep? HandledStep => BotStep.WaitForFlatForward;

    public override async Task Execute(Update update, ITelegramBotClient client)
    {
        if (update.Message == null) return;
        long chatId = update.Message.Chat.Id;

        try
        {
            if (update.Message.ForwardFromChat == null)
            {
                await SendChannelErrorAsync(chatId, client);
                return;
            }

            var hasContent = update.Message.Caption != null || update.Message.Text != null;

            if (update.Message.ForwardFromChat.Id != _botConfig.SourceChannelId)
            {
                if (hasContent)
                {
                    await SendChannelErrorAsync(chatId, client);
                }
                return;
            }

            if (!hasContent)
                return;

            var session = await _sessionStore.GetAsync(chatId);
            if (session?.RentalApplication == null)
                throw new SessionExpiredException();

            var error = await _forwardProcessor.ProcessAsync(chatId, update, client, session);
            if (!string.IsNullOrEmpty(error))
            {
                await _messageService.SendMessage(chatId, client, error, null);
            }
        }
        catch (SessionExpiredException)
        {
            await _messageService.SendMessage(chatId, client, SessionExpiredException.MessageText, null);
        }
        catch (ValidationException ex)
        {
            var msg = string.Join("\n", ex.Errors.Select(e => e.ErrorMessage));
            await _messageService.SendMessage(chatId, client, msg, null);
        }
    }

    private async Task SendChannelErrorAsync(long chatId, ITelegramBotClient client)
    {
        var body = await _messageResolver.ResolveAsync(chatId, "channelError", "Перешлите пост из канала @propertyintbilisi");
        await _messageService.SendMessage(chatId, client, body, null);
    }
}
