namespace Application.Flats.Interfaces;

public interface ICreateFlatCommand
{
    Task CreateFlatAsync(Flat flat);
}
