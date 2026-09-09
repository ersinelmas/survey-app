using System.Security.Claims;

namespace SurveyApp.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        return Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.FindFirstValue("IsAdmin") == "true";
    }
}
