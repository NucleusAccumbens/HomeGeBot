using Application.Common.Authorization;
using Application.Common.Results;
using Domain.Common;
using MediatR;

namespace Application.AdminManagement.Commands.GrantAdminRights;

public class GrantAdminRightsRequest : ISuperAdminCommand, IRequest<Result<GrantAdminRightsResult>>
{
    public ChatId SuperAdminChatId { get; set; }

    public ChatId TargetUserChatId { get; set; }
}
