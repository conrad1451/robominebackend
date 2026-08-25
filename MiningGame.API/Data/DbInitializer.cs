using MiningGame.API.Data;

namespace MiningGame.API.Data
{
    public static class DbInitializer
    {
        public static void SeedData(GameDbContext db)
        {
            // Ensures the database exists
            db.Database.EnsureCreated();

            // Example: Add initial reference data or default entries if empty
            // if (!db.Mines.Any()) { ... }
            
            db.SaveChanges();
            // Add initial database seeding logic here if tables are empty
        }
    }
}