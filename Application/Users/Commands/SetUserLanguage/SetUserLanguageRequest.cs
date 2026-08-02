using Application.Common.Results;
using Domain.Common;
using MediatR;

namespace Application.Users.Commands.SetUserLanguage;

public record SetUserLanguageRequest(ChatId ChatId, string Language) : IRequest<Result<SetUserLanguageResult>>;
