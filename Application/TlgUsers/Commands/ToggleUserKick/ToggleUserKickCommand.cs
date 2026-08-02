using Domain.Common;
using MediatR;

namespace Application.TlgUsers.Commands.ToggleUserKick;

public record ToggleUserKickCommand(ChatId ChatId) : IRequest;
