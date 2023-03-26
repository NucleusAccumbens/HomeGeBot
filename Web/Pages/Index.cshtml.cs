using Application.Clients.Interfaces;
using Application.Common.Interfaces;
using Application.Flats.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Web.Models;

namespace Web.Pages;

public class IndexModel : PageModel
{
    private readonly IBotDbContext _context;

    private readonly IGetClientDependencyQuery _getClientDependencyQuery;

    private readonly IGetFlatsQuery _getFlatsQuery;

    private readonly IUpdateFlatCommand _updateFlatCommand;

    private readonly IDeleteFlatCommand _deleteFlatCommand;

    public IndexModel(IBotDbContext context, IGetClientDependencyQuery getClientDependencyQuery,
        IGetFlatsQuery getFlatsQuery, IUpdateFlatCommand updateFlatCommand, IDeleteFlatCommand deleteFlatCommand)
    {
        _context = context;
        _getClientDependencyQuery = getClientDependencyQuery;
        _getFlatsQuery = getFlatsQuery;
        _updateFlatCommand = updateFlatCommand;
        _deleteFlatCommand = deleteFlatCommand;
    }

    public List<AppViewModel> Applications { get; set; } = new();

    public List<FlatViewModel> Flats { get; set; } = new();

    public List<ManagerViewModel> Managers { get; set; } = new();


    public async Task OnGetAsync()
    {
        Applications = await MapClientsToApplications();
        Flats = await MapFlatsToFlatDtos();
        Managers = await GetManagersDtoAsync();
    }

    public async Task<IActionResult> OnPostUpdateCommentAsync(string itemId, string comment)
    {
        await _updateFlatCommand.UpdateCommentAsync(itemId, comment);

        var flats = await MapFlatsToFlatDtos();

        var updatedFlat = flats.FirstOrDefault(f => f.ItemId == itemId);

        return Partial("_FlatRowPartial", updatedFlat);
    }

    public async Task<IActionResult> OnPostRemoveFlatAsync(string itemId)
    {
        await _deleteFlatCommand.DeleteFlatAsync(itemId);

        return RedirectToPage();
    }

    private async Task<List<AppViewModel>> MapClientsToApplications()
    {
        var applications = new List<AppViewModel>();

        var clients = await _context.Clients
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        foreach (var client in clients)
        {
            applications.Add(new AppViewModel()
            {
                Username = await _getClientDependencyQuery
                .GetClientUsernameAsync(client.ChatId),
                Country = client.Country.ToString(),
                Profession = client.Profession,
                HasPets = await GetHasPetsStringValue(client.HasPets),
                Term = client.Term.ToString(),
                ManagerUsername = await _getClientDependencyQuery
                .GetClientsManaderUsernameAsync(client.AdminChatId)
            });
        }

        return applications;
    }

    private async Task<List<FlatViewModel>> MapFlatsToFlatDtos()
    {
        var flatsDto = new List<FlatViewModel>();

        var flats = await _getFlatsQuery.GetFlatsAsync();

        foreach (var flat in flats)
        {
            flatsDto.Add(new FlatViewModel()
            {
                ItemId = flat.ItemId,
                PublicationDate = flat.CreatedAt.ToShortDateString(),
                Link = flat.Link,
                OwnerNumber = flat.OwnerNumber,
                Comment = flat.Comment
            });
        }

        return flatsDto;
    }

    private async Task<List<ManagerViewModel>> GetManagersDtoAsync()
    {
        var managers = new List<ManagerViewModel>();

        var admins = await _context.Admins
            .Include(u => u.Clients)
            .ToListAsync();

        foreach (var admin in admins)
        {
            var adminUsername = await _context.TlgUsers
                .Where(u => u.ChatId == admin.ChatId)
                .Select(u => u.Username)
                .SingleOrDefaultAsync();

            var clientUsernames = await GetClientsUsernames(admin.Clients);

            managers.Add(new ManagerViewModel()
            {
                Username = adminUsername,
                ClientUsernames = clientUsernames
            });
        }

        return managers;
    }

    private async Task<string> GetHasPetsStringValue(bool? hasPets)
    {
        var hasPersEntity = await _context.HasPets.FirstAsync();

        if (hasPets == true) return hasPersEntity.Yes;
        else return hasPersEntity.No;
    }

    private async Task<List<string>> GetClientsUsernames(List<Client> clents)
    {
        var usernames = new List<string>();

        foreach (var client in clents)
        {
            var username = await _context.TlgUsers
                .Where(u => u.ChatId == client.ChatId)
                .Select(u => u.Username)
                .SingleOrDefaultAsync();
            if (username != null)
            {
                usernames.Add(username);
            }

            else usernames.Add("Имя пользователя не найдено.");
        }

        return usernames;
    }

}
