namespace Application.Admins.Interfaces;

public interface IDeactivateAdminCommand
{
    Task DeactivateAdminAsync(long chatId);
}
