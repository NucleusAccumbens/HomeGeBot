using Application.Common.Authorization;
using Application.Common.Interfaces;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Behaviors;

public class ResultAuthorizationBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, Result<TResult>>
    where TRequest : IAdminCommand, IRequest<Result<TResult>>
{
    private readonly IBotDbContext _context;

    public ResultAuthorizationBehavior(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TResult>> Handle(
        TRequest request,
        RequestHandlerDelegate<Result<TResult>> next,
        CancellationToken cancellationToken)
    {
        var admin = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.AdminChatId && a.IsActive, cancellationToken);

        if (admin == null)
        {
            return Result<TResult>.Failure("Доступ запрещён.");
        }

        return await next();
    }
}
