namespace Application.Dashboard;

public class UpdateFlatCommentResult
{
    public bool Success { get; set; }

    public FlatDto? UpdatedFlat { get; set; }

    public string? ErrorMessage { get; set; }
}
