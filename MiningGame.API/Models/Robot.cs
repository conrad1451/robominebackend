namespace MiningGame.API.Models;

public enum RobotType
{
    Basic,
    Advanced,
    Elite
}

public class Robot
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public Guid? MineId { get; set; }
    public Mine? Mine { get; set; }
    public string Name { get; set; } = string.Empty;
    public RobotType Type { get; set; }
    public int Level { get; set; } = 1;
    public decimal Efficiency { get; set; }
    public bool IsWorking { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
