using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiningGame.API.Filters;
using MiningGame.API.Models;
using MiningGame.API.Services;

namespace MiningGame.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[TypeFilter(typeof(ResolvePlayerFilter))] // Automatically resolves Player before action runs
public class RobotsController : ControllerBase
{
    private readonly GameService _gameService;
    private readonly RobotService _robotService;

    public RobotsController(GameService gameService, RobotService robotService)
    {
        _gameService = gameService;
        _robotService = robotService;
    }

    [HttpPost]
    public async Task<ActionResult<GameStateDto>> BuyRobot([FromBody] BuyRobotRequest request)
    {
        if (!SnakeCaseEnum.TryParse<RobotType>(request.Type, out var robotType) ||
            !SnakeCaseEnum.TryParse<MineType>(request.MineType, out var mineType))
        {
            return BadRequest("Invalid robot or mine type.");
        }

        var player = this.GetCurrentPlayer();

        try
        {
            await _robotService.BuyRobotAsync(player.Id, robotType, mineType);
        }
        catch (GameLogicException ex) // Source 20
        {
            return BadRequest(ex.Message);
        }

        var refreshed = await _gameService.GetPlayerAsync(player.Id);
        return Ok(refreshed!.ToGameStateDto()); // Source 13
    }
}