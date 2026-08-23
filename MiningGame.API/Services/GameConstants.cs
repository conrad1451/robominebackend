namespace MiningGame.API.Services;

using MiningGame.API.Models;

// CHQ: Claude AI (Sonnet) generated code


// A recoverable, user-facing game-rule violation (insufficient funds, invalid
// state, etc.) - controllers catch this and return 400 with the message.
public class GameLogicException : Exception
{
    public GameLogicException(string message) : base(message) { }
}

public static class GameConstants
{
    public static readonly Dictionary<MineType, decimal> OreBaseValue = new()
    {
        { MineType.Gold, 70 },
        { MineType.Silver, 15 },
        { MineType.Copper, 12 },
        { MineType.Iron, 5 },
        { MineType.Lithium, 200 },
        { MineType.RareEarth, 300 }
    };

    public static readonly Dictionary<MaterialType, decimal> MaterialValues = new()
    {
        { MaterialType.RefinedGold, 450 },
        { MaterialType.RefinedSilver, 75 },
        { MaterialType.RefinedCopper, 12 },
        { MaterialType.Circuits, 320 },
        { MaterialType.Batteries, 280 },
        { MaterialType.ConstructionSteel, 5 }
    };

    public static readonly Dictionary<RobotType, decimal> RobotCosts = new()
    {
        { RobotType.Basic, 5000 },
        { RobotType.Advanced, 15000 },
        { RobotType.Elite, 50000 }
    };

    public static readonly Dictionary<RobotType, decimal> RobotEfficiency = new()
    {
        { RobotType.Basic, 1 },
        { RobotType.Advanced, 2.5m },
        { RobotType.Elite, 5 }
    };

    public static readonly Dictionary<RobotType, decimal> RobotUpgradeBaseCost = new()
    {
        { RobotType.Basic, 2000 },
        { RobotType.Advanced, 6000 },
        { RobotType.Elite, 20000 }
    };

    public const int MaxRobotLevel = 10;
    public const decimal MineUpgradeCost = 10000;

    // Cost to take a robot from its current level to level+1.
    public static decimal RobotUpgradeCost(RobotType type, int currentLevel) =>
        Math.Round(RobotUpgradeBaseCost[type] * (decimal)Math.Pow(1.6, currentLevel - 1));

    public static readonly List<ProcessingRecipe> Recipes = new()
    {
        new() { Id = MaterialType.RefinedGold, OutputType = MaterialType.RefinedGold, InputType = MineType.Gold, InputQuantity = 10, OutputQuantity = 5, EnergyCost = 500 },
        new() { Id = MaterialType.RefinedSilver, OutputType = MaterialType.RefinedSilver, InputType = MineType.Silver, InputQuantity = 10, OutputQuantity = 5, EnergyCost = 400 },
        new() { Id = MaterialType.RefinedCopper, OutputType = MaterialType.RefinedCopper, InputType = MineType.Copper, InputQuantity = 10, OutputQuantity = 5, EnergyCost = 300 },
        new() { Id = MaterialType.Circuits, OutputType = MaterialType.Circuits, InputType = MineType.RareEarth, InputQuantity = 6, OutputQuantity = 2, EnergyCost = 800 },
        new() { Id = MaterialType.Batteries, OutputType = MaterialType.Batteries, InputType = MineType.Lithium, InputQuantity = 8, OutputQuantity = 3, EnergyCost = 600 },
        new() { Id = MaterialType.ConstructionSteel, OutputType = MaterialType.ConstructionSteel, InputType = MineType.Iron, InputQuantity = 15, OutputQuantity = 5, EnergyCost = 400 },
    };
}

// The frontend uses snake_case strings ("rare_earth", "refined_gold") for
// enum values in route segments; this converts to/from the PascalCase C#
// enum names. JSON body serialization is handled separately by the global
// JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) in Program.cs.
public static class SnakeCaseEnum
{
    public static bool TryParse<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var pascal = string.Concat(
            value.Split('_', StringSplitOptions.RemoveEmptyEntries)
                 .Select(part => char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant())
        );

        return Enum.TryParse(pascal, ignoreCase: false, out result);
    }
}