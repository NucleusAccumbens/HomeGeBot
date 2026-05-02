using Application.Common.Interfaces;
using Application.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Models;

namespace Web.Pages;

public class IndexModel : PageModel
{
    private readonly IGetAdminDashboardUseCase _getDashboardUseCase;
    private readonly IUpdateFlatCommentUseCase _updateFlatCommentUseCase;
    private readonly IDeleteFlatUseCase _deleteFlatUseCase;

    public IndexModel(
        IGetAdminDashboardUseCase getDashboardUseCase,
        IUpdateFlatCommentUseCase updateFlatCommentUseCase,
        IDeleteFlatUseCase deleteFlatUseCase)
    {
        _getDashboardUseCase = getDashboardUseCase;
        _updateFlatCommentUseCase = updateFlatCommentUseCase;
        _deleteFlatUseCase = deleteFlatUseCase;
    }

    public List<AppViewModel> Applications { get; set; } = new();

    public List<FlatViewModel> Flats { get; set; } = new();

    public List<ManagerViewModel> Managers { get; set; } = new();


    public async Task OnGetAsync()
    {
        var adminChatId = GetCurrentAdminChatId();

        var result = await _getDashboardUseCase.ExecuteAsync(new GetAdminDashboardRequest
        {
            AdminChatId = adminChatId
        });

        if (result.Success)
        {
            Applications = result.Applications.Select(a => new AppViewModel
            {
                Username = a.Username ?? "",
                Country = a.Country?.ToString() ?? "",
                Profession = a.Profession ?? "",
                HasPets = a.HasPets ?? "",
                Term = a.Term?.ToString() ?? "",
                ManagerUsername = a.ManagerUsername ?? ""
            }).ToList();

            Flats = result.Flats.Select(f => new FlatViewModel
            {
                ItemId = f.ItemId,
                PublicationDate = f.PublicationDate,
                Link = f.Link,
                OwnerNumber = f.OwnerNumber,
                Comment = f.Comment
            }).ToList();

            Managers = result.Managers.Select(m => new ManagerViewModel
            {
                Username = m.Username,
                ClientUsernames = new List<string>()
            }).ToList();
        }
    }

    public async Task<IActionResult> OnPostUpdateCommentAsync(string itemId, string comment)
    {
        var adminChatId = GetCurrentAdminChatId();

        var result = await _updateFlatCommentUseCase.ExecuteAsync(new UpdateFlatCommentRequest
        {
            AdminChatId = adminChatId,
            ItemId = itemId,
            Comment = comment
        });

        if (!result.Success || result.UpdatedFlat == null)
        {
            return BadRequest(result.ErrorMessage);
        }

        var updatedFlat = new FlatViewModel
        {
            ItemId = result.UpdatedFlat.ItemId,
            PublicationDate = result.UpdatedFlat.PublicationDate,
            Link = result.UpdatedFlat.Link,
            OwnerNumber = result.UpdatedFlat.OwnerNumber,
            Comment = result.UpdatedFlat.Comment
        };

        return Partial("_FlatRowPartial", updatedFlat);
    }

    public async Task<IActionResult> OnPostRemoveFlatAsync(string itemId)
    {
        var adminChatId = GetCurrentAdminChatId();

        var result = await _deleteFlatUseCase.ExecuteAsync(new DeleteFlatRequest
        {
            AdminChatId = adminChatId,
            ItemId = itemId
        });

        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }

        return RedirectToPage();
    }

    private long GetCurrentAdminChatId()
    {
        // TODO: Получить chatId текущего залогиненного админа из сессии/claims
        // Временно возвращаем 0, нужно будет добавить аутентификацию
        return 0;
    }
}
