namespace MiningGame.API.Models;

public class Player
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 50000;
    public long TotalMined { get; set; } = 0;
    public long GameTime { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastPlayedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Mine> Mines { get; set; } = new List<Mine>();
    public ICollection<Robot> Robots { get; set; } = new List<Robot>();
    public ICollection<Material> Materials { get; set; } = new List<Material>();
}
