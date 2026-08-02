using Application.Common.Interfaces;
using Application.Common.Localization;
using Application.RentalApplications.Queries.GetUserApplications;
using Application.Users.Commands.SetUserLanguage;
using Application.Users.Queries.GetManagerContact;
using Application.Users.Queries.GetUserLanguage;
using Bot.Session;
using Domain.Common;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Filters;
using Web.Models;
using Web.Services;

namespace Web.Controllers;

[ApiController]
[Route("api/tma")]
[IgnoreAntiforgeryToken]
public class TmaController : TmaControllerBase
{
    private readonly IBotSessionStore _sessionStore;
    private readonly IUserNotifier _notifier;
    private readonly IMediator _mediator;
    private readonly ITmaLabelProvider _tmaLabels;

    public TmaController(
        IBotSessionStore sessionStore,
        IUserNotifier notifier,
        IMediator mediator,
        ITmaLabelProvider tmaLabels)
    {
        _sessionStore = sessionStore;
        _notifier = notifier;
        _mediator = mediator;
        _tmaLabels = tmaLabels;
    }

    [HttpPost("submit-application")]
    [ValidateTmaInitData]
    public async Task<IActionResult> SubmitApplication([FromBody] TmaApplicationRequest request)
    {
        var userId = TmaUserId;

        var session = new BotSession
        {
            ChatId = ChatId.FromLong(userId),
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

        var user = await _mediator.Send(new GetUserLanguageQuery(new ChatId(userId)));
        var labels = _tmaLabels.GetSubmitApplicationLabels(user);

        var message = $"{labels.Title}\n\n" +
            $"{labels.Country}: {countryDisplay}\n" +
            $"{labels.Profession}: {request.Profession}\n" +
            $"{labels.Pets}: {(request.HasPets ? labels.PetsYes : labels.PetsNo)}\n" +
            $"{labels.Term}: {termDisplay}\n\n" +
            $"{labels.Footer}";

        await _notifier.SendNotificationAsync(userId, message);

        return Ok(new { countryDisplay, termDisplay });
    }

    [HttpGet("applications")]
    [ValidateTmaInitData]
    public async Task<IActionResult> GetApplications([FromQuery] string initData)
    {
        var result = await _mediator.Send(new GetUserApplicationsRequest { ChatId = ChatId.FromLong(TmaUserId) });

        if (result.IsFailure)
        {
            return StatusCode(500, result.Error);
        }

        return Ok(result.Value!.Applications);
    }

    [HttpGet("manager")]
    [ValidateTmaInitData]
    public async Task<IActionResult> GetManager([FromQuery] string initData)
    {
        var result = await _mediator.Send(new GetManagerContactQuery());

        if (result.IsFailure)
        {
            return StatusCode(500, result.Error);
        }

        return Ok(new { username = result.Value!.Username });
    }

    [HttpGet("profile")]
    [ValidateTmaInitData]
    public IActionResult GetProfile([FromQuery] string initData)
    {
        var userData = TmaUserData;

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
    [ValidateTmaInitData]
    public async Task<IActionResult> SetLanguage([FromBody] TmaSetLanguageRequest request)
    {
        var result = await _mediator.Send(new SetUserLanguageRequest(new ChatId(TmaUserId), request.Language));

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }
}
