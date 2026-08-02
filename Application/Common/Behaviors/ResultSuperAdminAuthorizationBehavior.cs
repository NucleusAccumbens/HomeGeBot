using Application.Common.Authorization;
using Application.Common.Interfaces;
using Application.Common.Results;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Behaviors;

public class ResultSuperAdminAuthorizationBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, Result<TResult>>
    where TRequest : ISuperAdminCommand, IRequest<Result<TResult>>
{
    private readonly IBotDbContext _context;

    public ResultSuperAdminAuthorizationBehavior(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TResult>> Handle(
        TRequest request,
        RequestHandlerDelegate<Result<TResult>> next,
        CancellationToken cancellationToken)
    {
        var admin = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.SuperAdminChatId && a.IsActive, cancellationToken);

        if (admin == null || admin.Role != AdminRole.SuperAdmin)
        {
            return Result<TResult>.Failure("Доступ запрещён. Требуются права супер-администратора.");
        }

        return await next();
    }
}
