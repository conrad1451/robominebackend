using Microsoft.EntityFrameworkCore;
using MiningGame.API.Data;
using MiningGame.API.Models;

namespace MiningGame.API.Services;

// CHQ: Claude AI (Sonnet) generated code
public class MiningService
{
    private readonly GameDbContext _context;

    public MiningService(GameDbContext context)
    {
        _context = context;
    }

    // Sells all stockpiled ore in the given mine at its base per-unit value.
    public async Task<Player> SellOreAsync(Guid playerId, MineType mineType)
    {
        var player = await LoadPlayerAsync(playerId);

        var mine = player.Mines.FirstOrDefault(m => m.Type == mineType)
            ?? throw new GameLogicException($"No mine of type '{mineType}' found for this player.");

        if (mine.TotalExtracted <= 0)
        {
            throw new GameLogicException("This mine has no stockpiled ore to sell.");
        }

        var proceeds = mine.TotalExtracted * GameConstants.OreBaseValue[mineType];
        player.Balance += proceeds;
        mine.TotalExtracted = 0;

        await _context.SaveChangesAsync();
        return player;
    }

    // Flat-cost mine upgrade: deepens the mine, boosts extraction rate, and
    // increases stockpile capacity.
    public async Task<Player> UpgradeMineAsync(Guid playerId, MineType mineType)
    {
        var player = await LoadPlayerAsync(playerId);

        var mine = player.Mines.FirstOrDefault(m => m.Type == mineType)
            ?? throw new GameLogicException($"No mine of type '{mineType}' found for this player.");

        if (player.Balance < GameConstants.MineUpgradeCost)
        {
            throw new GameLogicException("Insufficient balance to upgrade this mine.");
        }

        player.Balance -= GameConstants.MineUpgradeCost;
        mine.Depth += 10;
        mine.ResourcePerSecond *= 1.2m;
        mine.MaxCapacity = (int)Math.Round(mine.MaxCapacity * 1.25m);

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