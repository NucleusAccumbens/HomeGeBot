namespace Application.AdminManagement;

public class GrantAdminRightsRequest
{
    public long SuperAdminChatId { get; set; }

    public long TargetUserChatId { get; set; }
}
