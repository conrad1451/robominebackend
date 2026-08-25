using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiningGame.API.Models;
using MiningGame.API.Services;

namespace MiningGame.API.Controllers;

// MiningGame.API/Controllers/GameStateController.cs


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GameStateController : ControllerBase
{
    private readonly GameService _gameService;
    private readonly ILogger<GameStateController> _logger;

    public GameStateController(GameService gameService, ILogger<GameStateController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    // GET /api/gamestate
    // Settles any resources accrued since the last collection, then returns
    // the player's full state: profile, mines, robots, and materials.
    [HttpGet]
    public async Task<ActionResult<GameStateDto>> Get()
    {
        var email = User.GetEmail();
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Authenticated request had no email claim.");
            return BadRequest("Token did not contain an email claim.");
        }

        var player = await _gameService.GetOrCreatePlayerByEmailAsync(email);

        await _gameService.CollectResourcesAsync(player.Id);

        var refreshed = await _gameService.GetPlayerAsync(player.Id);
        if (refreshed == null)
        {
            _logger.LogError("Player {PlayerId} disappeared after collection.", player.Id);
            return NotFound();
        }

        return Ok(refreshed.ToGameStateDto());
    }
}