using System.Security.Claims;

namespace Web.Services;

public interface IAdminClaimsFactory
{
    ClaimsPrincipal CreatePrincipal(long chatId, string username);
}

public class AdminClaimsFactory : IAdminClaimsFactory
{
    public ClaimsPrincipal CreatePrincipal(long chatId, string username)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, "Admin"),
            new("ChatId", chatId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "AdminAuth");
        return new ClaimsPrincipal(identity);
    }
}
