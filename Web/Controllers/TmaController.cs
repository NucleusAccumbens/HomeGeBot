using Application.Common.Interfaces;
using Application.Common.Localization;
using Application.RentalApplications.Queries.GetUserApplications;
using Bot.Session;
using Domain.Common;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Models;
using Web.Services;

namespace Web.Controllers;

[ApiController]
[Route("api/tma")]
[IgnoreAntiforgeryToken]
public class TmaController : ControllerBase
{
    private readonly ITmaValidationService _tmaValidation;
    private readonly IBotSessionStore _sessionStore;
    private readonly IUserNotifier _notifier;
    private readonly IMediator _mediator;
    private readonly IBotDbContext _dbContext;

    public TmaController(ITmaValidationService tmaValidation, IBotSessionStore sessionStore, IUserNotifier notifier, IMediator mediator, IBotDbContext dbContext)
    {
        _tmaValidation = tmaValidation;
        _sessionStore = sessionStore;
        _notifier = notifier;
        _mediator = mediator;
        _dbContext = dbContext;
    }

    [HttpPost("submit-application")]
    public async Task<IActionResult> SubmitApplication([FromBody] TmaApplicationRequest request)
    {
        if (!_tmaValidation.ValidateInitData(request.InitData))
        {
            return Unauthorized("Invalid initData");
        }

        var userId = _tmaValidation.GetUserId(request.InitData);
        if (userId == null)
        {
            return BadRequest("Could not find user ID in initData");
        }

        var session = new BotSession
        {
            ChatId = userId.Value,
            Step = BotStep.WaitForFlatForward,
            RentalApplication = new RentalApplicationDraft
            {
                Country = request.Country,
                CountryOther = request.CountryOther,
                Profession = request.Profession,
                HasPets = request.HasPets,
                Term = request.Term,
                TermOther = request.TermOther
            }
        };

        await _sessionStore.SaveAsync(session);

        var countryDisplay = request.Country == Country.Other && !string.IsNullOrWhiteSpace(request.CountryOther)
            ? request.CountryOther : request.Country.GetDisplayName();
        var termDisplay = request.Term == Term.Other && !string.IsNullOrWhiteSpace(request.TermOther)
            ? request.TermOther : request.Term.GetDisplayName();

        var user = await _dbContext.TlgUsers
            .Where(u => u.ChatId == new ChatId(userId.Value))
            .Select(u => u.Language)
            .FirstOrDefaultAsync() ?? "ru";

        var (petsYes, petsNo) = user switch
        {
            "en" => ("Yes", "No"),
            "ka" => ("დიახ", "არა"),
            _ => ("Да", "Нет")
        };

        var labels = user switch
        {
            "en" => new { Title = "Application data received!", Country = "Country", Profession = "Profession", Pets = "Pets", Term = "Rental term", Footer = "To complete the application, forward a property post from the @propertyintbilisi channel to this chat." },
            "ka" => new { Title = "განაცხადის მონაცემები მიღებულია!", Country = "ქვეყანა", Profession = "საქმიანობა", Pets = "შინაური ცხოველები", Term = "იჯარის ვადა", Footer = "განაცხადის დასასრულებლად, გადააგზავნეთ ბინის პოსტი @propertyintbilisi არხიდან ამ ჩატში." },
            _ => new { Title = "Данные заявки получены!", Country = "Страна", Profession = "Деятельность", Pets = "Домашние животные", Term = "Срок аренды", Footer = "Чтобы завершить заявку, перешлите пост с квартирой из канала @propertyintbilisi в этот чат." }
        };

        var message = $"{labels.Title}\n\n" +
            $"{labels.Country}: {countryDisplay}\n" +
            $"{labels.Profession}: {request.Profession}\n" +
            $"{labels.Pets}: {(request.HasPets ? petsYes : petsNo)}\n" +
            $"{labels.Term}: {termDisplay}\n\n" +
            $"{labels.Footer}";

        await _notifier.SendNotificationAsync(userId.Value, message);

        return Ok(new { countryDisplay, termDisplay });
    }

    [HttpGet("applications")]
    public async Task<IActionResult> GetApplications([FromQuery] string initData)
    {
        if (!_tmaValidation.ValidateInitData(initData))
        {
            return Unauthorized("Invalid initData");
        }

        var userId = _tmaValidation.GetUserId(initData);
        if (userId == null)
        {
            return BadRequest("Could not find user ID in initData");
        }

        var result = await _mediator.Send(new GetUserApplicationsRequest { ChatId = userId.Value });

        if (result.IsFailure)
        {
            return StatusCode(500, result.Error);
        }

        return Ok(result.Value!.Applications);
    }

    [HttpGet("manager")]
    public async Task<IActionResult> GetManager([FromQuery] string initData)
    {
        if (!_tmaValidation.ValidateInitData(initData))
        {
            return Unauthorized("Invalid initData");
        }

        var superAdmin = await _dbContext.Admins
            .AsNoTracking()
            .Where(a => a.Role == AdminRole.SuperAdmin && a.IsActive)
            .Select(a => new { a.ChatId })
            .FirstOrDefaultAsync();

        if (superAdmin == null)
        {
            return NotFound("Super admin not found");
        }

        var username = await _dbContext.TlgUsers
            .AsNoTracking()
            .Where(u => u.ChatId == superAdmin.ChatId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync();

        return Ok(new { username });
    }

    [HttpGet("profile")]
    public IActionResult GetProfile([FromQuery] string initData)
    {
        if (!_tmaValidation.ValidateInitData(initData))
        {
            return Unauthorized("Invalid initData");
        }

        var userData = _tmaValidation.GetUserData(initData);
        if (userData == null)
        {
            return BadRequest("Could not extract user data from initData");
        }

        return Ok(new
        {
            id = userData.Id,
            username = userData.Username,
            firstName = userData.FirstName,
            lastName = userData.LastName,
            photoUrl = userData.PhotoUrl
        });
    }

    [HttpPost("set-language")]
    public async Task<IActionResult> SetLanguage([FromBody] TmaSetLanguageRequest request)
    {
        if (!_tmaValidation.ValidateInitData(request.InitData))
        {
            return Unauthorized("Invalid initData");
        }

        var userId = _tmaValidation.GetUserId(request.InitData);
        if (userId == null)
        {
            return BadRequest("Could not find user ID in initData");
        }

        if (!SupportedLanguages.IsValid(request.Language))
        {
            return BadRequest("Invalid language code");
        }

        var user = await _dbContext.TlgUsers
            .Where(u => u.ChatId == new ChatId(userId.Value))
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound("User not found");
        }

        user.Language = request.Language;
        await _dbContext.SaveChangesAsync();

        return Ok();
    }
}
