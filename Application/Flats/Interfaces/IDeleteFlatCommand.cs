namespace Application.Flats.Interfaces;

public interface IDeleteFlatCommand
{
    Task DeleteFlatAsync(string itemId);
}
