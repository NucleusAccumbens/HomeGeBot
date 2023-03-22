namespace Application.Admins.Interfaces;

public interface IUpdateAdminCommand
{
    Task AddClientToAdminAsync(long chatId, Client client);
}
