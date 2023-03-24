using Application.Common.Interfaces;
using Application.Flats.Interfaces;

namespace Parser.Services;

public class FlatService
{
    private readonly ICheckFlatIsInBdQuery _checkFlatIsInBdQuery; 
    
    public FlatService(ICheckFlatIsInBdQuery checkFlatIsInBdQuery)
    {
        _checkFlatIsInBdQuery = checkFlatIsInBdQuery;
    }
}
