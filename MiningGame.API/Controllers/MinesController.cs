using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiningGame.API.Models;
using MiningGame.API.Services;

// MiningGame.API/Controllers/MinesController.cs
namespace MiningGame.API.Controllers;

// CHQ: Claude AI (Sonnet) generated code
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MinesController : ControllerBase
{
    private readonly GameService _gameService;
    private readonly MiningService _miningService;
    private readonly ILogger<MinesController> _logger;

    public MinesController(GameService gameService, MiningService miningService, ILogger<MinesController> logger)
    {
        _gameService = gameService;
        _miningService = miningService;
        _logger = logger;
    }

    // POST /api/mines/{mineType}/sell
    // Sells all stockpiled ore in the given mine (e.g. "rare_earth", "gold").
    [HttpPost("{mineType}/sell")]
    public async Task<ActionResult<GameStateDto>> SellOre(string mineType)
    {
        if (!SnakeCaseEnum.TryParse<MineType>(mineType, out var parsedType))
        {
            return BadRequest($"Unknown mine type '{mineType}'.");
        }

        var playerId = await ResolvePlayerIdAsync();
        if (playerId == null) return BadRequest("Token did not contain an email claim.");

        try
        {
            await _miningService.SellOreAsync(playerId.Value, parsedType);
        }
        catch (GameLogicException ex)
        {
            return BadRequest(ex.Message);
        }

        var refreshed = await _gameService.GetPlayerAsync(playerId.Value);
        return Ok(refreshed!.ToGameStateDto());
    }

    // POST /api/mines/{mineType}/upgrade
    [HttpPost("{mineType}/upgrade")]
    public async Task<ActionResult<GameStateDto>> UpgradeMine(string mineType)
    {
        if (!SnakeCaseEnum.TryParse<MineType>(mineType, out var parsedType))
        {
            return BadRequest($"Unknown mine type '{mineType}'.");
        }

        var playerId = await ResolvePlayerIdAsync();
        if (playerId == null) return BadRequest("Token did not contain an email claim.");

        try
        {
            await _miningService.UpgradeMineAsync(playerId.Value, parsedType);
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