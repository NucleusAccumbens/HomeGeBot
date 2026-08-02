using Application.Common.Authorization;
using Application.Common.Results;
using Domain.Common;
using MediatR;

namespace Application.AdminManagement.Commands.RevokeAdminRights;

public class RevokeAdminRightsRequest : ISuperAdminCommand, IRequest<Result<RevokeAdminRightsResult>>
{
    public ChatId SuperAdminChatId { get; set; }

    public ChatId TargetAdminChatId { get; set; }
}
