using Application.Tests.Common;
using Application.Users.Queries.GetManagerContact;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Users.Queries;

public class GetManagerContactHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_ActiveSuperAdminExists_ReturnsUsername()
    {
        var context = CreateContext();
        context.Admins.Add(new Admin { ChatId = new ChatId(1), Role = AdminRole.SuperAdmin });
        context.TlgUsers.Add(new TlgUser(new ChatId(1), username: "super_admin"));
        await context.SaveChangesAsync();

        var handler = new GetManagerContactHandler(context);
        var result = await handler.Handle(new GetManagerContactQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Username.Should().Be("super_admin");
    }

    [Fact]
    public async Task Handle_NoActiveSuperAdmin_ReturnsFailure()
    {
        var context = CreateContext();
        var handler = new GetManagerContactHandler(context);

        var result = await handler.Handle(new GetManagerContactQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Супер-администратор не найден");
    }
}
