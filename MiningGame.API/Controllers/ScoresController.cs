using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// MiningGame.API/Controllers/ScoresController.cs

// CHQ: Gemini AI created file

namespace MiningGame.API.Controllers
{
    [ApiController]
    [Route("api/scores")]
    [Authorize] // Enforces authentication consistent with the rest of your API
    public class ScoresController : ControllerBase
    {
        // GET: api/scores/game/sandbox
        [HttpGet("game/{gameMode}")]
        public async Task<IActionResult> GetScoresByGameMode(string gameMode)
        {
            // TODO: Query your repository/database filtering by gameMode
            var scores = new[]
            {
                new { username = "Player1", score = 15000, gameMode },
                new { username = "Player2", score = 12500, gameMode }
            };

            return Ok(scores);
        }
    }
}