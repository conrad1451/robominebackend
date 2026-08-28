using Microsoft.EntityFrameworkCore;
using MiningGame.API.Data;

namespace MiningGame.API.Data;

public static class DbInitializer
{
    public static async Task SeedDataAsync(GameDbContext db)
    {
        // Rely exclusively on migrations in production environments to maintain standard EF Core migration history tracking.
        // Add static reference/lookup table seeding here if missing.
        // Example:
        // if (!await db.Recipes.AnyAsync()) { ... }

        await db.SaveChangesAsync();
    }
}