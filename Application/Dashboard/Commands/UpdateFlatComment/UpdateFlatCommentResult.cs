using Application.Dashboard.Dtos;

namespace Application.Dashboard.Commands.UpdateFlatComment;

public class UpdateFlatCommentResult
{
    public static UpdateFlatCommentResult Success(FlatDto flat) => new() { UpdatedFlat = flat };
    public static UpdateFlatCommentResult Failure(string error) => new() { ErrorMessage = error };

    public FlatDto? UpdatedFlat { get; set; }

    public string? ErrorMessage { get; set; }
}
