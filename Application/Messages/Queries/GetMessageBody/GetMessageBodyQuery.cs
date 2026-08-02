using MediatR;

namespace Application.Messages.Queries.GetMessageBody;

public record GetMessageBodyQuery(string MessageName, string Language = "ru") : IRequest<string?>;
