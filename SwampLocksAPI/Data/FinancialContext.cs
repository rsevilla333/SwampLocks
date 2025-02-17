using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using DotNetEnv;


public class FinancialContext : DbContext
{
    public DbSet<Sector> Sectors { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<StockData> StockDataEntries { get; set; }

    public FinancialContext(DbContextOptions<FinancialContext> options) : base(options)
    {
    
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stock>()
            .HasOne(s => s.Sector)
            .WithMany(sector => sector.Stocks)
            .HasForeignKey(s => s.SectorName)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StockData>()
            .HasKey(sd => new { sd.Ticker, sd.Date }); // Composite primary key

        modelBuilder.Entity<StockData>()
            .HasOne(sd => sd.Stock)
            .WithMany(s => s.DataEntries)
            .HasForeignKey(sd => sd.Ticker)
            .OnDelete(DeleteBehavior.Cascade);
    }
}