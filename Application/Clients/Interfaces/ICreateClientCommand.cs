namespace Application.Clients.Interfaces;

public interface ICreateClientCommand
{
    Task<Client> CreateClientAsync(Client client);
}
