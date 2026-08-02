using Application.Common.Extensions;
using FluentAssertions;

namespace Application.Tests.Extensions;

public class UserExtensionsTests
{
    [Fact]
    public void GetFullName_BothNames_ReturnsCombined()
    {
        UserExtensions.GetFullName("Иван", "Петров").Should().Be("Иван Петров");
    }

    [Fact]
    public void GetFullName_FirstNameOnly_ReturnsFirstName()
    {
        UserExtensions.GetFullName("Иван", null).Should().Be("Иван");
    }

    [Fact]
    public void GetFullName_LastNameOnly_ReturnsLastName()
    {
        UserExtensions.GetFullName("", "Петров").Should().Be("Петров");
    }

    [Fact]
    public void GetFullName_BothEmpty_ReturnsNull()
    {
        UserExtensions.GetFullName("", "").Should().BeNull();
    }

    [Fact]
    public void GetFullName_BothNull_ReturnsNull()
    {
        UserExtensions.GetFullName(null, null).Should().BeNull();
    }

    [Fact]
    public void GetFullName_WhitespaceOnly_ReturnsNull()
    {
        UserExtensions.GetFullName("   ", "  ").Should().BeNull();
    }
}
