using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MiningGame.API.Services;

namespace MiningGame.API.Filters;

public class ResolvePlayerFilter : IAsyncActionFilter
{
    private readonly GameService _gameService;

    public ResolvePlayerFilter(GameService gameService)
    {
        _gameService = gameService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        var email = user.GetEmail(); // ClaimsPrincipalExtensions (Source 21)

        if (string.IsNullOrEmpty(email))
        {
            context.Result = new BadRequestObjectResult("Token did not contain an email claim.");
            return;
        }

        var player = await _gameService.GetOrCreatePlayerByEmailAsync(email);
        context.HttpContext.Items["Player"] = player;

        await next();
    }
}