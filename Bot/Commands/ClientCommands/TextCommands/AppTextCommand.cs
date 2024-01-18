using Application.Admins.Interfaces;
using Application.Clients.Interfaces;
using Application.Messages.Interfaces;
using Application.TlgUsers.Interfaces;
using Bot.Common.Abstractions;
using Bot.Common.Services;
using Bot.Exceptions;
using Domain.Entities;

namespace Bot.Commands.ClientCommands.TextCommands;

public class AppTextCommand : BaseTextCommand
{
    private readonly IMemoryCacheService _memoryCacheService;

    private readonly IGetAdminsQuery _getAdminsQuery;

    private readonly IUpdateAdminCommand _updateAdminCommand;

    private readonly ICreateClientCommand _createClientCommand;

    private readonly IGetMessageQuery _getMessageQuery;

    public AppTextCommand(IMemoryCacheService memoryCacheService, IGetAdminsQuery getAdminsQuery, 
        IUpdateAdminCommand updateAdminCommand, ICreateClientCommand createClientCommand, 
        IGetMessageQuery getMessageQuery)
    {
        _memoryCacheService = memoryCacheService;
        _getAdminsQuery = getAdminsQuery;
        _updateAdminCommand = updateAdminCommand;
        _createClientCommand = createClientCommand;
        _getMessageQuery = getMessageQuery;
    }

    public override string Name => "app";

    public override async Task Execute(Update update, ITelegramBotClient client)
    {
        if (update.Message != null)
        {
            long chatId = update.Message.Chat.Id;

            try
            {
                if (update.Message.ForwardFromChat != null)
                {
                    if (update.Message.ForwardFromChat.Id != -1001580911411)
                    {
                        if (update.Message.Caption != null)
                        {
                            var message = await _getMessageQuery.GetMessageAsync("channelError");

                            await MessageService.SendMessage(chatId, client, message.Body, null);

                            return;
                        }
                    }

                    if (update.Message.ForwardFromChat.Id == -1001580911411)
                    {                       
                        if (update.Message.Caption != null)
                        {                          
                            int messageId = _memoryCacheService.GetMessageIdFromMemoryCache(chatId);
                            var message = await _getMessageQuery.GetMessageAsync("app");
                            var createdClient = await CreateClientAsync(chatId);

                            await _updateAdminCommand.AddClientToAdminAsync(createdClient.AdminChatId, createdClient);
                            await client.ForwardMessageAsync(createdClient.AdminChatId, chatId, update.Message.MessageId);
                            await MessageService.SendMessage(createdClient.AdminChatId, client,
                                $"<b>Страна:</b> {createdClient.Country}\n" +
                                $"<b>Деятельность:</b> {createdClient.Profession}\n" +
                                $"<b>Домашние животные:</b> {GetHasPetsStringValue(createdClient)}\n" +
                                $"<b>Срок аренды:</b> {createdClient.Term}",
                                new(new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithUrl(text: "Написать", url: $"http://t.me/{update.Message.Chat.Username}"),
                                    },
                                }));

                            await MessageService.DeleteMessage(chatId, messageId, client);
                            await MessageService.SendMessage(chatId, client,
                                message.Body, null);

                            _memoryCacheService.RemoveMessageIdFromMemoryCache(chatId);
                            _memoryCacheService.RemoveClienteFromMemoryCache(chatId);

                            return;
                        }

                        return;
                    }
                }
                else
                {
                    if (update.Message.Caption != null)
                    {
                        var message = await _getMessageQuery.GetMessageAsync("channelError");
                        
                        await MessageService.SendMessage(chatId, client, message.Body, null);

                        return;
                    }
                }
            }
            catch (MemoryCacheException ex)
            {
                await ex.SendExceptionMessage(chatId, client);
            }
        }
    }

    private static string GetHasPetsStringValue(Client client)
    {
        if (client.HasPets == true) return "Да";
        else return "Нет";
    }

    private async Task<Client> CreateClientAsync(long chatId)
    {
        var serviceClient = _memoryCacheService.GetClientFromMemoryCache(chatId);
        long adminChatId = await _getAdminsQuery.GetAdminWithLeastClientCountAsync();
        serviceClient.AdminChatId = adminChatId;

        return await _createClientCommand.CreateClientAsync(serviceClient);
    }
}
