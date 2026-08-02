using Application.Common.Results;
using MediatR;

namespace Application.Users.Queries.GetManagerContact;

public record GetManagerContactQuery : IRequest<Result<GetManagerContactResult>>;
