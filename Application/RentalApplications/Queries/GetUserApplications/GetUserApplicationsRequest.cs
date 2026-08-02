using Application.Common.Results;
using Domain.Common;
using MediatR;

namespace Application.RentalApplications.Queries.GetUserApplications;

public class GetUserApplicationsRequest : IRequest<Result<GetUserApplicationsResult>>
{
    public ChatId ChatId { get; set; }
}
