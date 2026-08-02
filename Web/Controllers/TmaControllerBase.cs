using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Controllers;

public abstract class TmaControllerBase : ControllerBase
{
    protected string TmaInitData => (string)HttpContext.Items["TmaInitData"]!;
    protected long TmaUserId => (long)HttpContext.Items["TmaUserId"]!;
    protected TmaUserData TmaUserData => (TmaUserData)HttpContext.Items["TmaUserData"]!;
}
