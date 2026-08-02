using Application.Common.Authorization;
using Application.Common.Results;
using Domain.Common;
using MediatR;

namespace Application.Dashboard.Commands.UpdateFlatComment;

public class UpdateFlatCommentRequest : IAdminCommand, IRequest<Result<UpdateFlatCommentResult>>
{
    public ChatId AdminChatId { get; set; }

    public string ItemId { get; set; } = "";

    public string Comment { get; set; } = "";
}
