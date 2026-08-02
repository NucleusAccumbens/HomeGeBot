using Application.Common.Interfaces;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.SetUserLanguage;

public class SetUserLanguageHandler : IRequestHandler<SetUserLanguageRequest, Result<SetUserLanguageResult>>
{
    private readonly IBotDbContext _context;

    public SetUserLanguageHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SetUserLanguageResult>> Handle(SetUserLanguageRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.TlgUsers
            .SingleOrDefaultAsync(u => u.ChatId == request.ChatId, cancellationToken);

        if (user == null)
        {
            return Result<SetUserLanguageResult>.Failure("Пользователь не найден.");
        }

        user.Language = request.Language;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<SetUserLanguageResult>.Success(SetUserLanguageResult.Success());
    }
}
