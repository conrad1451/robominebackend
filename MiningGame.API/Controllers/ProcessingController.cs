using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiningGame.API.Models;
using MiningGame.API.Services;

namespace MiningGame.API.Controllers;

// CHQ: Claude AI (Sonnet) generated code
public record SellMaterialRequest(long? Quantity);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProcessingController : ControllerBase
{
    private readonly GameService _gameService;
    private readonly ProcessingService _processingService;
    private readonly ILogger<ProcessingController> _logger;

    public ProcessingController(GameService gameService, ProcessingService processingService, ILogger<ProcessingController> logger)
    {
        _gameService = gameService;
        _processingService = processingService;
        _logger = logger;
    }

    // POST /api/processing/{recipeId}
    // recipeId is a MaterialType, e.g. "refined_gold", "construction_steel".
    [HttpPost("{recipeId}")]
    public async Task<ActionResult<GameStateDto>> Process(string recipeId)
    {
        if (!SnakeCaseEnum.TryParse<MaterialType>(recipeId, out var parsedRecipeId))
        {
            return BadRequest($"Unknown recipe '{recipeId}'.");
        }

        var playerId = await ResolvePlayerIdAsync();
        if (playerId == null) return BadRequest("Token did not contain an email claim.");

        try
        {
            await _processingService.ProcessAsync(playerId.Value, parsedRecipeId);
        }
        catch (GameLogicException ex)
        {
            return BadRequest(ex.Message);
        }

        var refreshed = await _gameService.GetPlayerAsync(playerId.Value);
        return Ok(refreshed!.ToGameStateDto());
    }

    // POST /api/processing/materials/{materialType}/sell  body: { "quantity": 10 } (optional)
    [HttpPost("materials/{materialType}/sell")]
    public async Task<ActionResult<GameStateDto>> SellMaterial(string materialType, [FromBody] SellMaterialRequest? request)
    {
        if (!SnakeCaseEnum.TryParse<MaterialType>(materialType, out var parsedType))
        {
            return BadRequest($"Unknown material type '{materialType}'.");
        }

        var playerId = await ResolvePlayerIdAsync();
        if (playerId == null) return BadRequest("Token did not contain an email claim.");

        try
        {
            await _processingService.SellMaterialAsync(playerId.Value, parsedType, request?.Quantity);
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