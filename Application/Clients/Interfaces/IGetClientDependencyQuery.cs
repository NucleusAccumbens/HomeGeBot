namespace Application.Clients.Interfaces;

public interface IGetClientDependencyQuery
{
    Task<string> GetClientUsernameAsync(long chatId);
    Task<string> GetClientsManaderUsernameAsync(long managerChatId);
}
