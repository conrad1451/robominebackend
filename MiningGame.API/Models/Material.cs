namespace MiningGame.API.Models;

// MiningGame.API/Models/Material.cs

public enum MaterialType
{
    RefinedGold,
    RefinedSilver,
    RefinedCopper,
    Circuits,
    Batteries,
    ConstructionSteel
}

public class Material
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public MaterialType Type { get; set; }
    public long Quantity { get; set; } = 0;
    public decimal Value { get; set; }
}
