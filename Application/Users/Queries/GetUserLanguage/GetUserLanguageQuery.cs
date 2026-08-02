using Domain.Common;
using MediatR;

namespace Application.Users.Queries.GetUserLanguage;

public record GetUserLanguageQuery(ChatId ChatId) : IRequest<string>;
