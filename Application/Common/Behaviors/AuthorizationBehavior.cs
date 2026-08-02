using Application.Common.Authorization;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAdminCommand
    where TResponse : class
{
    private readonly IBotDbContext _context;

    public AuthorizationBehavior(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var admin = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.AdminChatId && a.IsActive, cancellationToken);

        if (admin == null)
        {
            throw new UnauthorizedAccessException("Доступ запрещён.");
        }

        return await next();
    }
}
