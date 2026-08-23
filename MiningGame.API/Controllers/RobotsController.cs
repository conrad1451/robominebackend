using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiningGame.API.Models;
using MiningGame.API.Services;

namespace MiningGame.API.Controllers;

// CHQ: Claude AI (Sonnet) generated code
public record BuyRobotRequest(string Type, string MineType);
public record AssignRobotRequest(string MineType);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RobotsController : ControllerBase
{
    private readonly GameService _gameService;
    private readonly RobotService _robotService;
    private readonly ILogger<RobotsController> _logger;

    public RobotsController(GameService gameService, RobotService robotService, ILogger<RobotsController> logger)
    {
        _gameService = gameService;
        _robotService = robotService;
        _logger = logger;
    }

    // POST /api/robots  body: { "type": "basic", "mineType": "gold" }
    [HttpPost]
    public async Task<ActionResult<GameStateDto>> BuyRobot([FromBody] BuyRobotRequest request)
    {
        if (!SnakeCaseEnum.TryParse<RobotType>(request.Type, out var robotType))
        {
            return BadRequest($"Unknown robot type '{request.Type}'.");
        }
        if (!SnakeCaseEnum.TryParse<MineType>(request.MineType, out var mineType))
        {
            return BadRequest($"Unknown mine type '{request.MineType}'.");
        }

        var playerId = await ResolvePlayerIdAsync();
        if (playerId == null) return BadRequest("Token did not contain an email claim.");

        try
        {
            await _robotService.BuyRobotAsync(playerId.Value, robotType, mineType);
        }
        catch (GameLogicException ex)
        {
            return BadRequest(ex.Message);
        }

        var refreshed = await _gameService.GetPlayerAsync(playerId.Value);
        return Ok(refreshed!.ToGameStateDto());
    }

    // POST /api/robots/{robotId}/assign  body: { "mineType": "gold" }
    [HttpPost("{robotId}/assign")]
    public async Task<ActionResult<GameStateDto>> AssignRobot(Guid robotId, [FromBody] AssignRobotRequest request)
    {
        if (!SnakeCaseEnum.TryParse<MineType>(request.MineType, out var mineType))
        {
            return BadRequest($"Unknown mine type '{request.MineType}'.");
        }

        var playerId = await ResolvePlayerIdAsync();
        if (playerId == null) return BadRequest("Token did not contain an email claim.");

        try
        {
            await _robotService.AssignRobotAsync(playerId.Value, robotId, mineType);
        }
        catch (GameLogicException ex)
        {
            return BadRequest(ex.Message);
        }

        var refreshed = await _gameService.GetPlayerAsync(playerId.Value);
        return Ok(refreshed!.ToGameStateDto());
    }

    // POST /api/robots/{robotId}/upgrade
    [HttpPost("{robotId}/upgrade")]
    public async Task<ActionResult<GameStateDto>> UpgradeRobot(Guid robotId)
    {
        var playerId = await ResolvePlayerIdAsync();
        if (playerId == null) return BadRequest("Token did not contain an email claim.");

        try
        {
            await _robotService.UpgradeRobotAsync(playerId.Value, robotId);
        }
        catch (GameLogicException ex)
        {
            return BadRequest(ex.Message);
        }

        var refreshed = await _gameService.GetPlayerAsync(playerId.Value);
        return Ok(refreshed!.ToGameStateDto());
    }

    private async Task<Guid?> ResolvePlayerIdAsync()
    {
        var email = User.GetEmail();
        if (string.IsNullOrEmpty(email)) return null;

        var player = await _gameService.GetOrCreatePlayerByEmailAsync(email);
        return player.Id;
    }
}