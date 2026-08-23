using Microsoft.EntityFrameworkCore;
using MiningGame.API.Data;
using MiningGame.API.Models;

namespace MiningGame.API.Services;

public class ProcessingService
{
    private readonly GameDbContext _context;

    public ProcessingService(GameDbContext context)
    {
        _context = context;
    }

    // Runs a processing recipe once: consumes ore from the matching mine's
    // stockpile and energy from balance, produces the output material.
    public async Task<Player> ProcessAsync(Guid playerId, MaterialType recipeId)
    {
        var player = await LoadPlayerAsync(playerId);

        var recipe = GameConstants.Recipes.FirstOrDefault(r => r.Id == recipeId)
            ?? throw new GameLogicException($"No recipe found for '{recipeId}'.");

        if (player.Balance < recipe.EnergyCost)
        {
            throw new GameLogicException("Insufficient balance to cover the energy cost.");
        }

        var mine = player.Mines.FirstOrDefault(m => m.Type == recipe.InputType)
            ?? throw new GameLogicException($"No mine of type '{recipe.InputType}' found for this player.");

        if (mine.TotalExtracted < recipe.InputQuantity)
        {
            throw new GameLogicException("Not enough stockpiled ore to run this recipe.");
        }

        var material = player.Materials.FirstOrDefault(m => m.Type == recipe.OutputType)
            ?? throw new GameLogicException($"No material record found for '{recipe.OutputType}'.");

        player.Balance -= recipe.EnergyCost;
        mine.TotalExtracted -= recipe.InputQuantity;
        material.Quantity += recipe.OutputQuantity;

        await _context.SaveChangesAsync();
        return player;
    }

    // Sells up to `quantity` units of a material (or the full stockpile if
    // quantity is omitted/exceeds it) at its per-unit value.
    public async Task<Player> SellMaterialAsync(Guid playerId, MaterialType materialType, long? quantity = null)
    {
        var player = await LoadPlayerAsync(playerId);

        var material = player.Materials.FirstOrDefault(m => m.Type == materialType)
            ?? throw new GameLogicException($"No material record found for '{materialType}'.");

        if (material.Quantity <= 0)
        {
            throw new GameLogicException("No stockpile of this material to sell.");
        }

        var sellQty = Math.Min(quantity ?? material.Quantity, material.Quantity);
        if (sellQty <= 0)
        {
            throw new GameLogicException("Quantity to sell must be greater than zero.");
        }

        player.Balance += sellQty * material.Value;
        material.Quantity -= sellQty;

        await _context.SaveChangesAsync();
        return player;
    }

    private async Task<Player> LoadPlayerAsync(Guid playerId)
    {
        return await _context.Players
            .Include(p => p.Mines)
            .Include(p => p.Robots)
            .Include(p => p.Materials)
            .FirstOrDefaultAsync(p => p.Id == playerId)
            ?? throw new GameLogicException("Player not found.");
    }
}