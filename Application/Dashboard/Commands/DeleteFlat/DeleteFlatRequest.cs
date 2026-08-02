using Application.Common.Authorization;
using Application.Common.Results;
using Domain.Common;
using MediatR;

namespace Application.Dashboard.Commands.DeleteFlat;

public class DeleteFlatRequest : IAdminCommand, IRequest<Result<DeleteFlatResult>>
{
    public ChatId AdminChatId { get; set; }

    public string ItemId { get; set; } = "";
}
