namespace Application.Flats.Interfaces;

public interface IUpdateFlatCommand
{
    Task UpdateCommentAsync(string itemId, string comment);
}
