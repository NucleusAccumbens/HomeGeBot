namespace Application.Admins.Interfaces;

public interface ICreateAdminCommand
{
    Task CreateAdminAsync(Admin admin);
}
