using Microsoft.EntityFrameworkCore;
using MiningGame.API.Data;
using MiningGame.API.Models;

namespace MiningGame.API.Services;

public class GameService
{
    private readonly GameDbContext _context;
    private readonly ILogger<GameService> _logger;

    private static readonly Dictionary<MineType, decimal> ORE_BASE_VALUE = new()
    {
        { MineType.Gold, 70 },
        { MineType.Silver, 15 },
        { MineType.Copper, 12 },
        { MineType.Iron, 5 },
        { MineType.Lithium, 200 },
        { MineType.RareEarth, 300 }
    };

    private static readonly Dictionary<MaterialType, decimal> MATERIAL_VALUES = new()
    {
        { MaterialType.RefinedGold, 450 },
        { MaterialType.RefinedSilver, 75 },
        { MaterialType.RefinedCopper, 12 },
        { MaterialType.Circuits, 320 },
        { MaterialType.Batteries, 280 },
        { MaterialType.ConstructionSteel, 5 }
    };

    public GameService(GameDbContext context, ILogger<GameService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Player?> GetPlayerAsync(Guid playerId)
    {
        return await _context.Players
            .Include(p => p.Mines)
            .Include(p => p.Robots)
            .Include(p => p.Materials)
            .FirstOrDefaultAsync(p => p.Id == playerId);
    }

    public async Task<Player> CreatePlayerAsync(string username, string email)
    {
        var player = new Player
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            Balance = 50000,
        };

        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        // Initialize mines
        var mineData = new[]
        {
            new { Name = "Golden Valley", Type = MineType.Gold, Depth = 100, ResourcePerSecond = 0.5m, MaxCapacity = 1000 },
            new { Name = "Silver Ridge", Type = MineType.Silver, Depth = 80, ResourcePerSecond = 0.8m, MaxCapacity = 1200 },
            new { Name = "Copper Canyon", Type = MineType.Copper, Depth = 120, ResourcePerSecond = 1.2m, MaxCapacity = 1500 },
            new { Name = "Lithium Deep", Type = MineType.Lithium, Depth = 200, ResourcePerSecond = 0.3m, MaxCapacity = 500 },
            new { Name = "Rare Element Core", Type = MineType.RareEarth, Depth = 300, ResourcePerSecond = 0.1m, MaxCapacity = 200 },
            new { Name = "Iron Ore Field", Type = MineType.Iron, Depth = 60, ResourcePerSecond = 2.5m, MaxCapacity = 2000 }
        };

        foreach (var data in mineData)
        {
            _context.Mines.Add(new Mine
            {
                Id = Guid.NewGuid(),
                PlayerId = player.Id,
                Name = data.Name,
                Type = data.Type,
                Depth = data.Depth,
                ResourcePerSecond = data.ResourcePerSecond,
                MaxCapacity = data.MaxCapacity
            });
        }

        // Initialize materials
        foreach (var materialType in Enum.GetValues<MaterialType>())
        {
            _context.Materials.Add(new Material
            {
                Id = Guid.NewGuid(),
                PlayerId = player.Id,
                Type = materialType,
                Value = MATERIAL_VALUES[materialType],
                Quantity = 0
            });
        }

        await _context.SaveChangesAsync();
        return player;
    }

// CHQ: Gemimi AI: handled calcuations as Anti-Cheat & Security measure
public async Task CollectResourcesAsync(Guid playerId)
{
    var player = await _context.Players
        .Include(p => p.Mines)
        .Include(p => p.Materials)
        .FirstOrDefaultAsync(p => p.Id == playerId);

    if (player == null)
    {
        _logger.LogWarning("Player {PlayerId} not found when attempting to collect resources.", playerId);
        return;
    }

    var now = DateTime.UtcNow;

    foreach (var mine in player.Mines)
    {
        // Calculate seconds elapsed since last collection tick (defaulting to current time if null)
        var lastCollected = mine.LastCollectedAt ?? now;
        var elapsedSeconds = (decimal)(now - lastCollected).TotalSeconds;

        if (elapsedSeconds <= 0) continue;

        // Calculate accrued resources capped at mine capacity
        var generatedAmount = Math.Min(elapsedSeconds * mine.ResourcePerSecond, mine.MaxCapacity);

        if (generatedAmount > 0)
        {
            // Locate target material corresponding to mine type
            var material = player.Materials.FirstOrDefault(m => m.Type.ToString() == mine.Type.ToString());
            if (material != null)
            {
                // CHQ: Gemini - variable labeled with long
                material.Quantity += (long)generatedAmount;
            }

            mine.LastCollectedAt = now;
        }
    }

    await _context.SaveChangesAsync();
}
}
    
