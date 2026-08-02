using Application.Common.Results;
using Domain.Common;
using MediatR;

namespace Application.BotStart.Commands.StartBot;

public class StartBotRequest : IRequest<Result<StartBotResult>>
{
    public ChatId ChatId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
