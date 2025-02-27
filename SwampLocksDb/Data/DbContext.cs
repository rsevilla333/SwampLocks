using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using DotNetEnv;
using SwampLocksDb.Models;

namespace SwampLocksDb.Data
{
public class FinancialContext : DbContext
{
    public DbSet<Sector> Sectors { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<StockData> StockDataEntries { get; set; }
    public DbSet<SectorPerformance> SectorPerformances { get; set; }
    public DbSet<Article> Articles { get; set; }
	public DbSet<ExchangeRate> ExchangeRates { get; set; }
    public DbSet<InterestRate> InterestRates { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        Env.Load();
        string? connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string is missing.");
        }

        
        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure primary key for Stock using Ticker
        modelBuilder.Entity<Stock>()
            .HasKey(s => s.Ticker); // Stock has Ticker as its primary key

        modelBuilder.Entity<Stock>()
            .HasOne(s => s.Sector)
            .WithMany(sector => sector.Stocks)
            .HasForeignKey(s => s.SectorName)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite key for StockData
        modelBuilder.Entity<StockData>()
            .HasKey(sd => new { sd.Ticker, sd.Date }); // Composite key for StockData

        modelBuilder.Entity<StockData>()
            .HasOne(sd => sd.Stock)
            .WithMany(s => s.DataEntries)
            .HasForeignKey(sd => sd.Ticker)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite key for Article
        modelBuilder.Entity<Article>()
            .HasKey(a => new { a.Ticker, a.ArticleName, a.Date }); // Composite key (Ticker + ArticleName + Date)

        modelBuilder.Entity<Article>()
            .HasOne(a => a.Stock)
            .WithMany(s => s.Articles)
            .HasForeignKey(a => a.Ticker)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure SectorPerformance composite key
        modelBuilder.Entity<SectorPerformance>()
            .HasKey(sp => new { sp.SectorName, sp.Date }); // Composite key for SectorPerformance

        modelBuilder.Entity<SectorPerformance>()
            .HasOne(sp => sp.Sector)
            .WithMany(s => s.Performances)
            .HasForeignKey(sp => sp.SectorName)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExchangeRate>()
                .HasKey(er => new { er.Date, er.TargetCurrency });

			modelBuilder.Entity<InterestRate>()
                .HasKey(r => r.Id);
    }
}
}