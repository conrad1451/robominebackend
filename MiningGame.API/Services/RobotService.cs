using Microsoft.EntityFrameworkCore;
using MiningGame.API.Data;
using MiningGame.API.Models;

namespace MiningGame.API.Services;

// MiningGame.API/Services/RobotService.cs

// CHQ: Claude AI (Sonnet) generated code
public class RobotService
{
    private readonly GameDbContext _context;

    public RobotService(GameDbContext context)
    {
        _context = context;
    }

    // Buys a new robot of the given type, assigned to work the given mine.
    public async Task<Player> BuyRobotAsync(Guid playerId, RobotType type, MineType mineType)
    {
        var player = await LoadPlayerAsync(playerId);

        var mine = player.Mines.FirstOrDefault(m => m.Type == mineType)
            ?? throw new GameLogicException($"No mine of type '{mineType}' found for this player.");

        var cost = GameConstants.RobotCosts[type];
        if (player.Balance < cost)
        {
            throw new GameLogicException("Insufficient balance to buy this robot.");
        }

        player.Balance -= cost;

        var robot = new Robot
        {
            Id = Guid.NewGuid(),
            PlayerId = player.Id,
            MineId = mine.Id,
            Name = $"{type} Bot #{player.Robots.Count + 1}",
            Type = type,
            Level = 1,
            Efficiency = GameConstants.RobotEfficiency[type],
            IsWorking = true
        };

        _context.Robots.Add(robot);
        mine.RobotsAssigned += 1;

        await _context.SaveChangesAsync();
        return await LoadPlayerAsync(playerId);
    }

    // Moves an existing robot to work a different mine.
    public async Task<Player> AssignRobotAsync(Guid playerId, Guid robotId, MineType mineType)
    {
        var player = await LoadPlayerAsync(playerId);

        var robot = player.Robots.FirstOrDefault(r => r.Id == robotId)
            ?? throw new GameLogicException("Robot not found for this player.");

        var newMine = player.Mines.FirstOrDefault(m => m.Type == mineType)
            ?? throw new GameLogicException($"No mine of type '{mineType}' found for this player.");

        var oldMine = player.Mines.FirstOrDefault(m => m.Id == robot.MineId);
        if (oldMine != null && oldMine.Id != newMine.Id)
        {
            oldMine.RobotsAssigned = Math.Max(oldMine.RobotsAssigned - 1, 0);
            newMine.RobotsAssigned += 1;
        }
        else if (oldMine == null)
        {
            newMine.RobotsAssigned += 1;
        }

        robot.MineId = newMine.Id;

        await _context.SaveChangesAsync();
        return player;
    }

    // Levels up a robot, increasing its efficiency. Cost scales with type and
    // current level; capped at GameConstants.MaxRobotLevel.
    public async Task<Player> UpgradeRobotAsync(Guid playerId, Guid robotId)
    {
        var player = await LoadPlayerAsync(playerId);

        var robot = player.Robots.FirstOrDefault(r => r.Id == robotId)
            ?? throw new GameLogicException("Robot not found for this player.");

        if (robot.Level >= GameConstants.MaxRobotLevel)
        {
            throw new GameLogicException("This robot is already at max level.");
        }

        var cost = GameConstants.RobotUpgradeCost(robot.Type, robot.Level);
        if (player.Balance < cost)
        {
            throw new GameLogicException("Insufficient balance to upgrade this robot.");
        }

        player.Balance -= cost;
        robot.Level += 1;
        robot.Efficiency += GameConstants.RobotEfficiency[robot.Type] * 0.5m;

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