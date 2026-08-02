using Application.Common.Results;
using Application.Tests.Common;
using Application.Users.Commands.SetUserLanguage;
using Domain.Common;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Users.Commands;

public class SetUserLanguageHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public async Task Handle_UserExists_UpdatesLanguage()
    {
        var context = CreateContext();
        context.TlgUsers.Add(new TlgUser(new ChatId(100), username: "user", language: "ru"));
        await context.SaveChangesAsync();

        var handler = new SetUserLanguageHandler(context);
        var result = await handler.Handle(new SetUserLanguageRequest(new ChatId(100), "en"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var user = await context.TlgUsers.SingleAsync();
        user.Language.Should().Be("en");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        var context = CreateContext();
        var handler = new SetUserLanguageHandler(context);

        var result = await handler.Handle(new SetUserLanguageRequest(new ChatId(999), "en"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Пользователь не найден");
    }
}
