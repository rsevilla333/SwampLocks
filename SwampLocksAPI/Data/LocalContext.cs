using Microsoft.EntityFrameworkCore;

namespace SwampLocksAPI.Data
{
    public class LocalContext : DbContext
    {
        public DbSet<Stock> Stocks { get; set; }

        public LocalContext(DbContextOptions<LocalContext> options) : base(options)    
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Stock>().HasData(new Stock { Ticker = "AMZN", SectorName = "TECH" });
            modelBuilder.Entity<Stock>().HasData(new Stock { Ticker = "GOOGL", SectorName = "TECH" });
        }
    }
}
