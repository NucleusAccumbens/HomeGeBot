using Application.Common.Results;
using Application.Dashboard.Commands.GetAdminDashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Models;

using Microsoft.AspNetCore.Authorization;

using Application.AdminManagement.Commands.CloseRequest;
using Application.AdminManagement.Commands.GetBotUsers;
using Application.AdminManagement.Commands.GrantAdminRights;
using Application.AdminManagement.Commands.ReassignClient;
using Application.AdminManagement.Commands.RevokeAdminRights;
using Application.AdminManagement.Dtos;

using Domain.Common;
using Domain.Enums;

namespace Web.Pages;

[Authorize(AuthenticationSchemes = "AdminAuth")]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public List<AppViewModel> Applications { get; set; } = new();

    public List<ManagerViewModel> Managers { get; set; } = new();

    public List<BotUserDto> BotUsers { get; set; } = new();

    public bool IsSuperAdmin { get; set; }

    public ManagerProfileViewModel ManagerProfile { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Tab { get; set; }

    [BindProperty(SupportsGet = true)]
    public long? ViewManager { get; set; }


    public async Task OnGetAsync()
    {
        var adminChatId = GetCurrentAdminChatId();

        var result = await _mediator.Send(new GetAdminDashboardRequest
        {
            AdminChatId = adminChatId
        });

        if (result.IsSuccess)
        {
            IsSuperAdmin = result.Value!.IsSuperAdmin;

            Applications = result.Value.Applications.Select(a => new AppViewModel
            {
                Id = a.Id,
                ClientChatId = a.ChatId,
                Username = a.Username ?? "",
                Country = a.Country == Domain.Enums.Country.Other && !string.IsNullOrWhiteSpace(a.CountryOther)
                    ? a.CountryOther : a.Country?.GetDisplayName() ?? "",
                Profession = a.Profession ?? "",
                HasPets = a.HasPets ?? "",
                Term = a.Term == Domain.Enums.Term.Other && !string.IsNullOrWhiteSpace(a.TermOther)
                    ? a.TermOther : a.Term?.GetDisplayName() ?? "",
                ManagerUsername = a.ManagerUsername ?? "",
                Name = a.ManagerName ?? "",
                IsCompleted = a.IsCompleted
            }).ToList();

            Managers = result.Value.Managers.Select(m => new ManagerViewModel
            {
                ChatId = m.ChatId,
                Username = m.Username,
                Name = m.Name,
                ApplicationCount = m.ApplicationCount,
                IsSuperAdmin = m.IsSuperAdmin
            }).ToList();

            ManagerProfile = new ManagerProfileViewModel
            {
                ChatId = adminChatId,
                Name = result.Value.CurrentAdminName ?? "",
                Username = result.Value.CurrentAdminUsername ?? "",
                ApplicationCount = result.Value.Applications.Count(a => !a.IsCompleted)
            };

            if (ViewManager.HasValue && ViewManager.Value > 0)
            {
                var mgr = Managers.FirstOrDefault(m => m.ChatId == ViewManager.Value);
                if (mgr != null)
                {
                    ManagerProfile = new ManagerProfileViewModel
                    {
                        ChatId = mgr.ChatId,
                        Name = mgr.Name ?? "",
                        Username = mgr.Username ?? "",
                        ApplicationCount = mgr.ApplicationCount
                    };
                }
            }
        }

        if (IsSuperAdmin)
        {
            var usersResult = await _mediator.Send(new GetBotUsersRequest
            {
                SuperAdminChatId = adminChatId
            });

            if (usersResult.IsSuccess)
            {
                BotUsers = usersResult.Value!.Users;
            }
        }
    }

    public async Task<IActionResult> OnPostGrantAdminAsync(long chatId, string? tab)
    {
        var adminChatId = GetCurrentAdminChatId();
        var result = await _mediator.Send(new GrantAdminRightsRequest
        {
            SuperAdminChatId = adminChatId,
            TargetUserChatId = chatId
        });

        return RedirectToPage(new { tab });
    }

    public async Task<IActionResult> OnPostRevokeAdminAsync(long chatId, string? tab)
    {
        var adminChatId = GetCurrentAdminChatId();
        var result = await _mediator.Send(new RevokeAdminRightsRequest
        {
            SuperAdminChatId = adminChatId,
            TargetAdminChatId = chatId
        });

        return RedirectToPage(new { tab });
    }

    public async Task<IActionResult> OnPostReassignAsync(long clientChatId, long newManagerChatId, string? tab)
    {
        var adminChatId = GetCurrentAdminChatId();
        var result = await _mediator.Send(new ReassignClientRequest
        {
            SuperAdminChatId = adminChatId,
            ClientChatId = clientChatId,
            NewManagerChatId = newManagerChatId
        });

        return RedirectToPage(new { tab });
    }

    public async Task<IActionResult> OnPostCloseRequestAsync(long clientId, long clientChatId, string? tab)
    {
        var adminChatId = GetCurrentAdminChatId();
        var result = await _mediator.Send(new CloseRequestRequest
        {
            AdminChatId = adminChatId,
            ClientChatId = clientChatId,
            ClientId = clientId
        });

        return RedirectToPage(new { tab });
    }

    private long GetCurrentAdminChatId()
    {
        var chatIdClaim = User.FindFirst("ChatId")?.Value;
        return long.TryParse(chatIdClaim, out var chatId) ? chatId : 0;
    }
}
