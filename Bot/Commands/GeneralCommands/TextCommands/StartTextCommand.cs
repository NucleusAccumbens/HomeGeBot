using Application.TlgUsers.Interfaces;
using Bot.Common.Abstractions;
using Bot.Messages.ClientMessages;
using Bot.Messages.GeneralMessages;

namespace Bot.Commands.GeneralCommands.TextCommands;

public class StartTextCommand : BaseTextCommand
{
    private readonly CountryMessage _startMessage;

    private readonly AdminStartMessage _adminStartMessage;

    private readonly ICreateTlgUserCommand _createTlgUserCommand;

    private readonly ICheckUserIsInDbQuery _checkUserIsInDbQuery;

    private readonly ICheckUserIsAdminQuery _checkUserIsAdminQuery;
    public override string Name => "/start";

    public StartTextCommand(CountryMessage startMessage, ICreateTlgUserCommand createTlgUserCommand, 
        ICheckUserIsInDbQuery checkUserIsInDbQuery, ICheckUserIsAdminQuery checkUserIsAdminQuery, 
        AdminStartMessage adminStartMessage)
    {
        _startMessage = startMessage;
        _createTlgUserCommand = createTlgUserCommand;
        _checkUserIsInDbQuery = checkUserIsInDbQuery;
        _checkUserIsAdminQuery = checkUserIsAdminQuery;
        _adminStartMessage = adminStartMessage;
    }

    public override async Task Execute(Update update, ITelegramBotClient client)
    {
        if (update.Message != null)
        {
            long chatId = update.Message.Chat.Id;

            bool oldUser = await _checkUserIsInDbQuery.CheckUserIsInDbAsync(chatId);

            if (!oldUser)
            {
                await _createTlgUserCommand.CreateTlgUserAsync(update.Message.Chat);
            }

            bool? isAdmin = await _checkUserIsAdminQuery.CheckUserIsAdminAsync(chatId);

            if (isAdmin == true)
            {
                await _adminStartMessage.SendMessage(chatId, client);

                return;
            }

            await _startMessage.SendMessage(chatId, client);
        }
    }
}
