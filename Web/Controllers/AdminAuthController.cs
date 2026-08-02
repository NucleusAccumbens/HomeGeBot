using Application.AdminManagement.Queries.CheckAdminStatus;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Web.Filters;
using Web.Services;

namespace Web.Controllers;

[ApiController]
[Route("api/admin")]
[IgnoreAntiforgeryToken]
public class AdminAuthController : TmaControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;
    private readonly IAdminClaimsFactory _claimsFactory;

    public AdminAuthController(
        IMediator mediator,
        IWebHostEnvironment environment,
        IAdminClaimsFactory claimsFactory)
    {
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
    [ValidateTmaInitData]
    public async Task<IActionResult> Auth([FromBody] AdminAuthRequest request)
    {
        var isAdmin = await _mediator.Send(new CheckAdminStatusQuery(TmaUserId));
        if (!isAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access denied. You are not an administrator.");
        }

        var username = TmaUserData.Username ?? TmaUserData.FirstName ?? "Admin";
        var principal = _claimsFactory.CreatePrincipal(TmaUserId, username);
        await HttpContext.SignInAsync("AdminAuth", principal, new AuthenticationProperties { IsPersistent = true });

        return Ok();
    }
}

public class AdminAuthRequest
{
    public string InitData { get; set; } = string.Empty;
}
