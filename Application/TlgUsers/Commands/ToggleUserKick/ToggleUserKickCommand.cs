using Application.Common.Results;
using Domain.Common;
using MediatR;

namespace Application.TlgUsers.Commands.ToggleUserKick;

public record ToggleUserKickCommand(ChatId ChatId) : IRequest<Result<ToggleUserKickResult>>;
