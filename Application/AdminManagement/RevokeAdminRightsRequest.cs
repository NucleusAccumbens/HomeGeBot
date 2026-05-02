namespace Application.AdminManagement;

public class RevokeAdminRightsRequest
{
    public long SuperAdminChatId { get; set; }

    public long TargetAdminChatId { get; set; }
}
