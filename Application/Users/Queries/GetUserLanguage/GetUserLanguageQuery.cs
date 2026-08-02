using MediatR;

namespace Application.Users.Queries.GetUserLanguage;

public record GetUserLanguageQuery(long ChatId) : IRequest<string>;
