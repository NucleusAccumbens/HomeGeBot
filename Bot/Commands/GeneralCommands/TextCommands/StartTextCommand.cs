using Application.BotStart.Commands.StartBot;
using Bot.Common.Abstractions;
using Bot.Messages.ClientMessages;
using Bot.Messages.GeneralMessages;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bot.Commands.GeneralCommands.TextCommands;

public class StartTextCommand : BaseTextCommand
{
    private readonly ClientStartMessage _clientStartMessage;

    private readonly AdminStartMessage _adminStartMessage;

    private readonly ManagerStartMessage _managerStartMessage;

    private readonly IMediator _mediator;
    public override string Name => "/start";

    private readonly ILogger<StartTextCommand> _logger;

    public StartTextCommand(ClientStartMessage clientStartMessage, IMediator mediator,
        AdminStartMessage adminStartMessage, ManagerStartMessage managerStartMessage, ILogger<StartTextCommand> logger)
    {
        _clientStartMessage = clientStartMessage;
        _mediator = mediator;
        _adminStartMessage = adminStartMessage;
        _managerStartMessage = managerStartMessage;
        _logger = logger;
    }

    public override async Task Execute(Update update, ITelegramBotClient client)
    {
        if (update.Message != null)
        {
            long chatId = update.Message.Chat.Id;

            var result = await _mediator.Send(new StartBotRequest()
            {
                ChatId = chatId,
                Username = update.Message.Chat.Username,
                FirstName = update.Message.Chat.FirstName,
                LastName = update.Message.Chat.LastName,
            });

            _logger.LogInformation("StartTextCommand: User ChatId {ChatId}, Username {Username}, Admin status from result {IsAdmin}", 
                chatId, update.Message.Chat.Username, result.Value!.IsAdmin);

            if (result.Value.IsAdmin)
            {
                if (result.Value.IsSuperAdmin)
                {
                    _logger.LogInformation("StartTextCommand: Sending AdminStartMessage to SuperAdmin {ChatId}", chatId);
                    await _adminStartMessage.SendMessage(chatId, client);
                }
                else
                {
                    _logger.LogInformation("StartTextCommand: Sending ManagerStartMessage to Manager {ChatId}", chatId);
                    await _managerStartMessage.SendMessage(chatId, client);
                }
                return;
            }

            _logger.LogInformation("StartTextCommand: Sending ClientStartMessage to {ChatId}", chatId);
            await _clientStartMessage.SendMessage(chatId, client);
        }
    }
}
