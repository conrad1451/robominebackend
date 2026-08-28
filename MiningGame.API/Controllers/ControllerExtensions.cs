using Microsoft.AspNetCore.Mvc;
using MiningGame.API.Models;

namespace MiningGame.API.Controllers;

public static class ControllerExtensions
{
    public static Player GetCurrentPlayer(this ControllerBase controller)
    {
        if (controller.HttpContext.Items["Player"] is Player player)
        {
            return player;
        }
        
        throw new InvalidOperationException("Player context was not resolved. Ensure [TypeFilter(typeof(ResolvePlayerFilter))] is present on the controller or action.");
    }
}