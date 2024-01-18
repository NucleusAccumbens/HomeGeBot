namespace Application.Admins.Interfaces;

public interface IGetAdminsQuery
{
    Task<List<long>> GetAdminsChatIdsAsync();

    Task<long> GetAdminWithLeastClientCountAsync();
}
