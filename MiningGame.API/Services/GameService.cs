using Microsoft.EntityFrameworkCore;
using MiningGame.API.Data;
using MiningGame.API.Models;

namespace MiningGame.API.Services;

public class GameService    
{
    private readonly GameDbContext _context;
    private readonly ILogger<GameService> _logger;

    // Direct mapping from raw MineType to target MaterialType stored in player inventory
    private static readonly Dictionary<MineType, MaterialType> MINE_TO_MATERIAL_MAP = new()
    {
        { MineType.Gold, MaterialType.RefinedGold },
        { MineType.Silver, MaterialType.RefinedSilver },
        { MineType.Copper, MaterialType.RefinedCopper },
        { MineType.Iron, MaterialType.ConstructionSteel },
        { MineType.Lithium, MaterialType.Batteries },
        { MineType.RareEarth, MaterialType.Circuits }
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

    public async Task<Player?> GetPlayerByEmailAsync(string email)
    {
        return await _context.Players
            .Include(p => p.Mines)
            .Include(p => p.Robots)
            .Include(p => p.Materials)
            .FirstOrDefaultAsync(p => p.Email == email);
    }

    public async Task<Player> GetOrCreatePlayerByEmailAsync(string email, string? preferredUsername = null)
    {
        var existing = await GetPlayerByEmailAsync(email);
        if (existing != null)
        {
            existing.LastPlayedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }

        var username = preferredUsername ?? email.Split('@')[0];

        var baseUsername = username;
        var suffix = 1;
        while (await _context.Players.AnyAsync(p => p.Username == username))
        {
            username = $"{baseUsername}{suffix++}";
        }

        var created = await CreatePlayerAsync(username, email);
        return (await GetPlayerAsync(created.Id))!;
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

        foreach (var materialType in Enum.GetValues<MaterialType>())
        {
            _context.Materials.Add(new Material
            {
                Id = Guid.NewGuid(),
                PlayerId = player.Id,
                Type = materialType,
                Value = GameConstants.MaterialValues[materialType], // Replaced local duplicate with GameConstants
                Quantity = 0
            });
        }

        await _context.SaveChangesAsync();
        return player;
    }

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
            var lastCollected = mine.LastCollectedAt ?? now;
            var elapsedSeconds = (decimal)(now - lastCollected).TotalSeconds;

            if (elapsedSeconds <= 0) continue;

            var generatedAmount = Math.Min(elapsedSeconds * mine.ResourcePerSecond, mine.MaxCapacity);

            if (generatedAmount > 0)
            {
                // Key Fix: Correctly lookup target material type using MINE_TO_MATERIAL_MAP
                if (MINE_TO_MATERIAL_MAP.TryGetValue(mine.Type, out var targetMaterialType))
                {
                    var material = player.Materials.FirstOrDefault(m => m.Type == targetMaterialType);
                    if (material != null)
                    {
                        material.Quantity += (long)generatedAmount;
                    }
                }

                mine.LastCollectedAt = now;
            }
        }

        await _context.SaveChangesAsync();
    }
}