using Application.Common.Authorization;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Behaviors;

public class SuperAdminAuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ISuperAdminCommand
    where TResponse : class
{
    private readonly IBotDbContext _context;

    public SuperAdminAuthorizationBehavior(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var admin = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.SuperAdminChatId && a.IsActive, cancellationToken);

        if (admin == null || admin.Role != AdminRole.SuperAdmin)
        {
            throw new UnauthorizedAccessException("Доступ запрещён. Требуются права супер-администратора.");
        }

        return await next();
    }
}
