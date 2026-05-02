using Application.BotStart;
using Bot.Common.Abstractions;
using Bot.Messages.ClientMessages;
using Bot.Messages.GeneralMessages;

namespace Bot.Commands.GeneralCommands.TextCommands;

public class StartTextCommand : BaseTextCommand
{
    private readonly CountryMessage _startMessage;

    private readonly AdminStartMessage _adminStartMessage;

    private readonly IStartBotUseCase _startBotUseCase;
    public override string Name => "/start";

    public StartTextCommand(CountryMessage startMessage, IStartBotUseCase startBotUseCase, 
        AdminStartMessage adminStartMessage)
    {
        _startMessage = startMessage;
        _startBotUseCase = startBotUseCase;
        _adminStartMessage = adminStartMessage;
    }

    public override async Task Execute(Update update, ITelegramBotClient client)
    {
        if (update.Message != null)
        {
            long chatId = update.Message.Chat.Id;

            var result = await _startBotUseCase.ExecuteAsync(new StartBotRequest()
            {
                ChatId = chatId,
                Username = update.Message.Chat.Username,
            });

            if (result.IsKicked)
                return;

            if (result.IsAdmin)
            {
                await _adminStartMessage.SendMessage(chatId, client);
                return;
            }

            await _startMessage.SendMessage(chatId, client);
        }
    }
}
