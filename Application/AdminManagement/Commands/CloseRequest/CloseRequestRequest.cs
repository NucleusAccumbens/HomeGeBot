using Application.Common.Results;
using Domain.Common;
using MediatR;

namespace Application.AdminManagement.Commands.CloseRequest;

public class CloseRequestRequest : IRequest<Result<CloseRequestResult>>
{
    public ChatId AdminChatId { get; set; }
    public ChatId ClientChatId { get; set; }
    public long ClientId { get; set; }
}
