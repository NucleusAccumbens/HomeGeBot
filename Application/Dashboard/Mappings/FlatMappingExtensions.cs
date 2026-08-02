using Application.Dashboard.Dtos;
using Domain.Entities;

namespace Application.Dashboard.Mappings;

public static class FlatMappingExtensions
{
    public static FlatDto ToDto(this Flat flat)
    {
        return new FlatDto
        {
            ItemId = flat.ItemId ?? "",
            PublicationDate = flat.CreatedAt.ToShortDateString(),
            Link = flat.Link ?? "",
            OwnerNumber = flat.OwnerNumber ?? "",
            Comment = flat.Comment
        };
    }
}
