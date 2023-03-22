namespace Application.Flats.Interfaces;

public interface IGetFlatsQuery
{
    Task<List<Flat>> GetFlatsAsync();
}
