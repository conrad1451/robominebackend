using Microsoft.EntityFrameworkCore;
using MiningGame.API.Models;

namespace MiningGame.API.Data;

// MiningGame.API/Data/GameDbContext.cs

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Mine> Mines { get; set; } = null!;
    public DbSet<Robot> Robots { get; set; } = null!;
    public DbSet<Material> Materials { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Player configuration
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Email).IsUnique();
            entity.HasIndex(p => p.Username).IsUnique();
            entity.Property(p => p.Balance).HasColumnType("numeric(18,2)");
        });

        // Mine configuration
        modelBuilder.Entity<Mine>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.ResourcePerSecond).HasColumnType("numeric(10,2)");
            entity.Property(m => m.TotalExtracted).HasColumnType("numeric(18,2)");
            entity.HasOne(m => m.Player)
                .WithMany(p => p.Mines)
                .HasForeignKey(m => m.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Robot configuration
        modelBuilder.Entity<Robot>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Efficiency).HasColumnType("numeric(10,2)");
            entity.HasOne(r => r.Player)
                .WithMany(p => p.Robots)
                .HasForeignKey(r => r.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(r => r.Mine)
                .WithMany(m => m.Robots)
                .HasForeignKey(r => r.MineId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Material configuration
        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Value).HasColumnType("numeric(10,2)");
            entity.HasOne(m => m.Player)
                .WithMany(p => p.Materials)
                .HasForeignKey(m => m.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(m => new { m.PlayerId, m.Type }).IsUnique();
        });
    }
}
