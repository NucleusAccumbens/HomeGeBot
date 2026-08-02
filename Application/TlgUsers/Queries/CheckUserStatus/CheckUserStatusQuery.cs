using Domain.Common;
using MediatR;

namespace Application.TlgUsers.Queries.CheckUserStatus;

public record CheckUserStatusQuery(ChatId? ChatId) : IRequest<bool>;
