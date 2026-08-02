using Application.Dashboard.Dtos;
using Application.Dashboard.Mappings;
using Domain.Entities;
using FluentAssertions;

namespace Application.Tests.Mappings;

public class FlatMappingExtensionsTests
{
    [Fact]
    public void ToDto_MapsAllFields()
    {
        var flat = new Flat(
            itemId: "abc123",
            link: "https://example.com/flat",
            ownerNumber: "+995555123456",
            comment: "Хорошая квартира")
        {
            CreatedAt = new DateTime(2026, 7, 15)
        };

        var dto = flat.ToDto();

        dto.ItemId.Should().Be("abc123");
        dto.Link.Should().Be("https://example.com/flat");
        dto.OwnerNumber.Should().Be("+995555123456");
        dto.Comment.Should().Be("Хорошая квартира");
        dto.PublicationDate.Should().Be(new DateTime(2026, 7, 15).ToShortDateString());
    }

    [Fact]
    public void ToDto_NullFields_DefaultsToEmptyStrings()
    {
        var flat = new Flat(null, null, null, null)
        {
            CreatedAt = new DateTime(2026, 7, 15)
        };

        var dto = flat.ToDto();

        dto.ItemId.Should().Be("");
        dto.Link.Should().Be("");
        dto.OwnerNumber.Should().Be("");
        dto.Comment.Should().BeNull();
    }
}
