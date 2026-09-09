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

    public static Guid? TryGetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
