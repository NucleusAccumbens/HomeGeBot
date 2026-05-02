namespace Application.Dashboard;

public class UpdateFlatCommentRequest
{
    public long AdminChatId { get; set; }

    public string ItemId { get; set; } = "";

    public string Comment { get; set; } = "";
}
