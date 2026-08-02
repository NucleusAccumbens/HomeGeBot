using MediatR;

namespace Application.Messages.Queries.GetMessagePathToPhoto;

public record GetMessagePathToPhotoQuery(string MessageName) : IRequest<string?>;
