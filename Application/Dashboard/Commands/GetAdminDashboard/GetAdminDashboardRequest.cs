using Application.Common.Authorization;
using Application.Common.Results;
using Domain.Common;
using MediatR;

namespace Application.Dashboard.Commands.GetAdminDashboard;

public class GetAdminDashboardRequest : IAdminCommand, IRequest<Result<GetAdminDashboardResult>>
{
    public ChatId AdminChatId { get; set; }
}
