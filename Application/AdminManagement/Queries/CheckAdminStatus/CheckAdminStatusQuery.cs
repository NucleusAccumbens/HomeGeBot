using Domain.Common;
using MediatR;

namespace Application.AdminManagement.Queries.CheckAdminStatus;

public record CheckAdminStatusQuery(ChatId ChatId) : IRequest<bool>;
