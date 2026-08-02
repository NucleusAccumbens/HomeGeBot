using Application.Common.Authorization;
using Application.Common.Results;
using Domain.Common;
using MediatR;

namespace Application.AdminManagement.Commands.GetBotUsers;

public class GetBotUsersRequest : ISuperAdminCommand, IRequest<Result<GetBotUsersResult>>
{
    public ChatId SuperAdminChatId { get; set; }

    public string? SearchQuery { get; set; }
}
