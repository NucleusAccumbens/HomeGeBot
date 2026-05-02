namespace Application.AdminManagement;

public class GetBotUsersRequest
{
    public long RequestorChatId { get; set; }

    public string? SearchQuery { get; set; }
}
