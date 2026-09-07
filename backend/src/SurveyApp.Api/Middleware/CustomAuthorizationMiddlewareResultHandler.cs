using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace SurveyApp.Api.Middleware;

public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            var response = new { message = "Bu işlem için yetkiniz yok." };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
