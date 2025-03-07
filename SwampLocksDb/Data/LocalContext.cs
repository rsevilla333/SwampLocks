using Microsoft.EntityFrameworkCore;
using SwampLocksDb.Models;

namespace SwampLocksDb.Data
{
    public class LocalContext : DbContext
    {
        public DbSet<Sector> Sectors { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockData> StockDataEntries { get; set; }
        public DbSet<SectorPerformance> SectorPerformances { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<InterestRate> InterestRates { get; set; }

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
