using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiningGame.API.Models;
using MiningGame.API.Services;

namespace MiningGame.API.Controllers;

// MiningGame.API/Controllers/AuthController.cs


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly GameService _gameService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(GameService gameService, ILogger<AuthController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    // GET /api/auth/me
    // Returns the Player linked to the current Descope-authenticated account,
    // creating one with starter mines/materials on first login.
    [HttpGet("me")]
    public async Task<ActionResult<PlayerDto>> Me()
    {
        var email = User.GetEmail();
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Authenticated request had no email claim.");
            return BadRequest("Token did not contain an email claim.");
        }

        var player = await _gameService.GetOrCreatePlayerByEmailAsync(email);
        return Ok(player.ToDto());
    }
}