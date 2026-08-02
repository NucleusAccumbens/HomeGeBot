using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Results;
using Application.Dashboard.Dtos;
using Application.Dashboard.Mappings;
using Domain.Common;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Dashboard.Commands.GetAdminDashboard;

public class GetAdminDashboardHandler : IRequestHandler<GetAdminDashboardRequest, Result<GetAdminDashboardResult>>
{
    private readonly IBotDbContext _context;

    public GetAdminDashboardHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetAdminDashboardResult>> Handle(GetAdminDashboardRequest request, CancellationToken cancellationToken)
    {
        var admin = await _context.Admins
            .AsNoTracking()
            .SingleOrDefaultAsync(a => a.ChatId == request.AdminChatId && a.IsActive, cancellationToken);

        if (admin == null)
        {
            return Result<GetAdminDashboardResult>.Failure("Доступ запрещён.");
        }

        var isSuperAdmin = admin.Role == AdminRole.SuperAdmin;

        var adminUser = await _context.TlgUsers
            .AsNoTracking()
            .Where(u => u.ChatId == request.AdminChatId)
            .Select(u => new { u.Username, u.FirstName, u.LastName })
            .FirstOrDefaultAsync(cancellationToken);

        var adminName = UserExtensions.GetFullName(adminUser?.FirstName, adminUser?.LastName);

        List<ApplicationDto> applications;
        List<FlatDto> flats;
        List<ManagerDto> managers;

        if (isSuperAdmin)
        {
            applications = await GetApplicationsAsync(cancellationToken);
            flats = await GetFlatsAsync(cancellationToken);
            managers = await GetManagersAsync(cancellationToken);
        }
        else
        {
            applications = await GetApplicationsForManagerAsync(admin.ChatId, cancellationToken);
            flats = new List<FlatDto>();
            managers = new List<ManagerDto>();
        }

        return Result<GetAdminDashboardResult>.Success(
            GetAdminDashboardResult.Success(applications, flats, managers, isSuperAdmin, adminName, adminUser?.Username));
    }

    private async Task<List<ApplicationDto>> GetApplicationsAsync(CancellationToken cancellationToken)
    {
        var clients = await _context.Clients
            .AsNoTracking()
            .Include(c => c.Admin)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var clientChatIds = clients.Select(c => c.ChatId).ToList();
        var adminChatIds = clients.Select(c => c.Admin.ChatId).Distinct().ToList();

        var userInfo = await _context.TlgUsers
            .AsNoTracking()
            .Where(u => clientChatIds.Contains(u.ChatId) || adminChatIds.Contains(u.ChatId))
            .Select(u => new { u.ChatId, u.Username, u.FirstName, u.LastName })
            .ToDictionaryAsync(u => u.ChatId, cancellationToken);

        return clients.Select(c => new ApplicationDto
        {
            Id = c.Id,
            ChatId = c.ChatId,
            Username = userInfo.GetValueOrDefault(c.ChatId)?.Username,
            Country = c.Country,
            CountryOther = c.CountryOther,
            Profession = c.Profession,
            HasPets = c.HasPets == null ? "—" : c.HasPets.Value ? "Да" : "Нет",
            Term = c.Term,
            TermOther = c.TermOther,
            ManagerUsername = userInfo.GetValueOrDefault(c.Admin.ChatId)?.Username,
            ManagerName = UserExtensions.GetFullName(userInfo.GetValueOrDefault(c.Admin.ChatId)?.FirstName, userInfo.GetValueOrDefault(c.Admin.ChatId)?.LastName),
            IsCompleted = c.IsCompleted
        }).ToList();
    }

    private async Task<List<ApplicationDto>> GetApplicationsForManagerAsync(ChatId managerChatId, CancellationToken cancellationToken)
    {
        var clients = await _context.Clients
            .AsNoTracking()
            .Include(c => c.Admin)
            .Where(c => c.Admin.ChatId == managerChatId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var clientChatIds = clients.Select(c => c.ChatId).ToList();

        var userInfo = await _context.TlgUsers
            .AsNoTracking()
            .Where(u => clientChatIds.Contains(u.ChatId))
            .Select(u => new { u.ChatId, u.Username })
            .ToDictionaryAsync(u => u.ChatId, cancellationToken);

        return clients.Select(c => new ApplicationDto
        {
            Id = c.Id,
            ChatId = c.ChatId,
            Username = userInfo.GetValueOrDefault(c.ChatId)?.Username,
            Country = c.Country,
            CountryOther = c.CountryOther,
            Profession = c.Profession,
            HasPets = c.HasPets == null ? "—" : c.HasPets.Value ? "Да" : "Нет",
            Term = c.Term,
            TermOther = c.TermOther,
            IsCompleted = c.IsCompleted
        }).ToList();
    }

    private async Task<List<FlatDto>> GetFlatsAsync(CancellationToken cancellationToken)
    {
        return await _context.Flats
            .AsNoTracking()
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FlatDto
            {
                ItemId = f.ItemId ?? "",
                PublicationDate = f.CreatedAt.ToShortDateString(),
                Link = f.Link ?? "",
                OwnerNumber = f.OwnerNumber ?? "",
                Comment = f.Comment
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<ManagerDto>> GetManagersAsync(CancellationToken cancellationToken)
    {
        var admins = await _context.Admins
            .AsNoTracking()
            .Include(a => a.Clients)
            .Where(a => a.IsActive)
            .ToListAsync(cancellationToken);

        var adminChatIds = admins.Select(a => a.ChatId).ToList();

        var userInfo = await _context.TlgUsers
            .AsNoTracking()
            .Where(u => adminChatIds.Contains(u.ChatId))
            .Select(u => new { u.ChatId, u.Username, u.FirstName, u.LastName })
            .ToDictionaryAsync(u => u.ChatId, cancellationToken);

        return admins.Select(a =>
        {
            var info = userInfo.GetValueOrDefault(a.ChatId);
            return new ManagerDto
            {
                ChatId = a.ChatId,
                Username = info?.Username,
                Name = UserExtensions.GetFullName(info?.FirstName, info?.LastName),
                ApplicationCount = a.Clients.Count(c => !c.IsCompleted),
                IsSuperAdmin = a.Role == AdminRole.SuperAdmin
            };
        }).ToList();
    }
}
