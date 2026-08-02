using Application.Common.Results;
using Application.RentalApplications.Commands.SubmitRentalApplication;
using Application.Tests.Common;
using Application.Users.Queries.GetUserLanguage;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Application.Tests.RentalApplications;

public class SubmitRentalApplicationHandlerTests
{
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    private static SubmitRentalApplicationRequest CreateRequest(long chatId = 100)
    {
        return new SubmitRentalApplicationRequest
        {
            ChatId = new ChatId(chatId),
            Country = Country.Other,
            CountryOther = "France",
            Profession = "Engineer",
            HasPets = false,
            Term = Term.SixMonths,
            TermOther = null
        };
    }

    [Fact]
    public async Task Handle_NoActiveManager_ReturnsFailure()
    {
        var context = CreateContext();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetUserLanguageQuery>(), Arg.Any<CancellationToken>())
            .Returns("ru");

        var handler = new SubmitRentalApplicationHandler(context, mediator);

        var result = await handler.Handle(CreateRequest(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Нет активных менеджеров");
    }

    [Fact]
    public async Task Handle_WithActiveManager_AssignsAndReturnsSuccess()
    {
        var context = CreateContext();
        var manager = new Admin
        {
            ChatId = new ChatId(999),
            Role = AdminRole.Admin
        };
        context.Admins.Add(manager);
        context.TlgUsers.Add(new TlgUser { ChatId = new ChatId(100), Username = "client_user" });
        context.TlgUsers.Add(new TlgUser { ChatId = new ChatId(999), Username = "manager_user" });
        await context.SaveChangesAsync();

        var mediator = Substitute.For<IMediator>();
        var handler = new SubmitRentalApplicationHandler(context, mediator);

        var result = await handler.Handle(CreateRequest(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AssignedManagerChatId.Should().Be(new ChatId(999));
        result.Value.ManagerUsername.Should().Be("manager_user");
        result.Value.ClientUsername.Should().Be("client_user");

        var savedClient = await context.Clients.SingleAsync();
        savedClient.AdminChatId.Should().Be(new ChatId(999));
        savedClient.Profession.Should().Be("Engineer");
    }

    [Fact]
    public async Task Handle_FiveExistingApplications_ReturnsLimitFailure()
    {
        var context = CreateContext();
        var manager = new Admin
        {
            ChatId = new ChatId(999),
            Role = AdminRole.Admin
        };
        context.Admins.Add(manager);
        for (int i = 0; i < 5; i++)
        {
            context.Clients.Add(new Client
            {
                ChatId = new ChatId(100),
                AdminChatId = new ChatId(999)
            });
        }
        await context.SaveChangesAsync();

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetUserLanguageQuery>(), Arg.Any<CancellationToken>())
            .Returns("en");

        var handler = new SubmitRentalApplicationHandler(context, mediator);

        var result = await handler.Handle(CreateRequest(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("5 applications");
    }

    [Fact]
    public async Task Handle_PicksManagerWithFewestClients()
    {
        var context = CreateContext();
        var busyManager = new Admin
        {
            ChatId = new ChatId(1),
            Role = AdminRole.Admin
        };
        busyManager.AssignClient(new() { ChatId = new ChatId(10), AdminChatId = new ChatId(1) });
        busyManager.AssignClient(new() { ChatId = new ChatId(11), AdminChatId = new ChatId(1) });
        var freeManager = new Admin
        {
            ChatId = new ChatId(2),
            Role = AdminRole.Admin
        };
        context.Admins.AddRange(busyManager, freeManager);
        await context.SaveChangesAsync();

        var mediator = Substitute.For<IMediator>();
        var handler = new SubmitRentalApplicationHandler(context, mediator);

        var result = await handler.Handle(CreateRequest(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AssignedManagerChatId.Should().Be(new ChatId(2));
    }
}


