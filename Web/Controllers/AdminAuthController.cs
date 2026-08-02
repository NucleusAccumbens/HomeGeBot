using Application.AdminManagement.Queries.CheckAdminStatus;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Controllers;

[ApiController]
[Route("api/admin")]
[IgnoreAntiforgeryToken]
public class AdminAuthController : ControllerBase
{
    private readonly ITmaValidationService _tmaValidation;
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;
    private readonly IAdminClaimsFactory _claimsFactory;

    public AdminAuthController(ITmaValidationService tmaValidation, IMediator mediator,
        IWebHostEnvironment environment, IAdminClaimsFactory claimsFactory)
    {
        _tmaValidation = tmaValidation;
        _mediator = mediator;
        _environment = environment;
        _claimsFactory = claimsFactory;
    }

    [HttpGet("auth-debug")]
    public async Task<IActionResult> AuthDebug([FromQuery] long chatId)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var isAdmin = await _mediator.Send(new CheckAdminStatusQuery(chatId));
        if (!isAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Debug Access denied. User is not an administrator.");
        }

        var principal = _claimsFactory.CreatePrincipal(chatId, $"DebugAdmin_{chatId}");
        await HttpContext.SignInAsync("AdminAuth", principal, new AuthenticationProperties { IsPersistent = true });

        return Ok();
    }

    [HttpPost("auth")]
    public async Task<IActionResult> Auth([FromBody] AdminAuthRequest request)
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

        var isAdmin = await _mediator.Send(new CheckAdminStatusQuery(userId.Value));
        if (!isAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access denied. You are not an administrator.");
        }

        var username = _tmaValidation.GetUsername(request.InitData) ?? "Admin";
        var principal = _claimsFactory.CreatePrincipal(userId.Value, username);
        await HttpContext.SignInAsync("AdminAuth", principal, new AuthenticationProperties { IsPersistent = true });

        return Ok();
    }
}

public class AdminAuthRequest
{
    public string InitData { get; set; } = string.Empty;
}
