using Application.Common.Authorization;
using Application.Common.Results;
using Domain.Common;
using MediatR;

namespace Application.AdminManagement.Commands.ReassignClient;

public class ReassignClientRequest : ISuperAdminCommand, IRequest<Result<ReassignClientResult>>
{
    public ChatId SuperAdminChatId { get; set; }
    public ChatId ClientChatId { get; set; }
    public ChatId NewManagerChatId { get; set; }
}
