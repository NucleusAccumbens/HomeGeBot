namespace Application.Flats.Interfaces;

public interface ICheckFlatIsInBdQuery
{
    Task<bool> CheckFlatIsInBdAsync(string id);
}
