namespace MiningGame.API.Models;

// MiningGame.API/Models/Dtos.cs

// CHQ: Claude AI (Sonnet) generated code

public record PlayerDto(
    Guid Id,
    string Username,
    string Email,
    decimal Balance,
    long TotalMined,
    long GameTime,
    DateTime CreatedAt,
    DateTime LastPlayedAt
);

public record MineDto(
    Guid Id,
    string Name,
    MineType Type,
    int Depth,
    decimal ResourcePerSecond,
    decimal TotalExtracted,
    long LifetimeExtracted,
    int RobotsAssigned,
    int MaxCapacity,
    DateTime? LastCollectedAt
);

public record RobotDto(
    Guid Id,
    Guid? MineId,
    string Name,
    RobotType Type,
    int Level,
    decimal Efficiency,
    bool IsWorking
);

public record MaterialDto(
    Guid Id,
    MaterialType Type,
    long Quantity,
    decimal Value
);

public record GameStateDto(
    PlayerDto Player,
    List<MineDto> Mines,
    List<RobotDto> Robots,
    List<MaterialDto> Materials
);

public static class DtoMapping
{
    public static PlayerDto ToDto(this Player p) => new(
        p.Id, p.Username, p.Email, p.Balance, p.TotalMined, p.GameTime, p.CreatedAt, p.LastPlayedAt
    );

    public static MineDto ToDto(this Mine m) => new(
        m.Id, m.Name, m.Type, m.Depth, m.ResourcePerSecond, m.TotalExtracted,
        m.LifetimeExtracted, m.RobotsAssigned, m.MaxCapacity, m.LastCollectedAt
    );

    public static RobotDto ToDto(this Robot r) => new(
        r.Id, r.MineId, r.Name, r.Type, r.Level, r.Efficiency, r.IsWorking
    );

    public static MaterialDto ToDto(this Material m) => new(
        m.Id, m.Type, m.Quantity, m.Value
    );

    public static GameStateDto ToGameStateDto(this Player p) => new(
        p.ToDto(),
        p.Mines.Select(m => m.ToDto()).ToList(),
        p.Robots.Select(r => r.ToDto()).ToList(),
        p.Materials.Select(mat => mat.ToDto()).ToList()
    );
}