using Application.Admins.Interfaces;
using Bot.Common.Abstractions;
using Bot.Common.Services;

namespace Bot.Commands.GeneralCommands.TextCommands;

public class RemoveAdminTextCommand : BaseTextCommand
{
    private readonly IDeactivateAdminCommand _deactivateAdminCommand;

    public RemoveAdminTextCommand(IDeactivateAdminCommand deactivateAdminCommand)
    {
        _deactivateAdminCommand = deactivateAdminCommand;
    }

    public override string Name => "/removeAdmin";

    public override async Task Execute(Update update, ITelegramBotClient client)
    {
        if (update.Message != null)
        {
            long chatId = update.Message.Chat.Id;

            await _deactivateAdminCommand.DeactivateAdminAsync(chatId);

            await MessageService
                .SendMessage(chatId, client, "Права администратора аннулированы.", null);
        }
    }
}
