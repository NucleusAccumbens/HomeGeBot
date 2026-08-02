using Application.Dashboard.Dtos;

namespace Application.Dashboard.Commands.UpdateFlatComment;

public class UpdateFlatCommentResult
{
    public static UpdateFlatCommentResult Success(FlatDto flat) => new() { UpdatedFlat = flat };

    public FlatDto? UpdatedFlat { get; set; }
}
