namespace MiningGame.API.Models;

// MiningGame.API/Models/Mine.cs

public enum MineType
{
    Gold,
    Silver,
    Copper,
    Lithium,
    RareEarth,
    Iron
}

public class Mine
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public MineType Type { get; set; }
    public int Depth { get; set; }
    public decimal ResourcePerSecond { get; set; }
    public decimal TotalExtracted { get; set; } = 0;
    public long LifetimeExtracted { get; set; } = 0;
    public int RobotsAssigned { get; set; } = 0;
    public int MaxCapacity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // CHQ: Gemini AI: Add missing LastCollectedAt property
    public DateTime? LastCollectedAt { get; set; }
    public ICollection<Robot> Robots { get; set; } = new List<Robot>();
}
