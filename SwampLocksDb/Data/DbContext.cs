using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using DotNetEnv;
using SwampLocksDb.Models;
using Microsoft.Data.SqlClient;
using Azure.Identity;
using Azure.Core;

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
    public DbSet<StockBalanceSheet> StockBalanceSheets { get; set; }
    public DbSet<CashFlowStatement> CashFlowStatements { get; set; }
    public DbSet<IncomeStatement> IncomeStatements { get; set; }
    public DbSet<StockEarningStatement> StockEarnings { get; set; }
    public DbSet<EconomicIndicator> EconomicIndicators { get; set; }
    public DbSet<EconomicData> EconomicDataPoints { get; set; }
	public DbSet<CommodityIndicator> Commodities { get; set; }
    public DbSet<CommodityData> CommodityDataPoints { get; set; }
	public DbSet<DataUpdateTracker> DataUpdateTrackers { get; set; }  
    
    public FinancialContext(DbContextOptions<FinancialContext> options) : base(options)
    {
    }
    
    public FinancialContext()
    {
    }
    
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;
        Env.Load();

		string databaseName = Environment.GetEnvironmentVariable("DB_NAME") ?? "";
		string serverName = Environment.GetEnvironmentVariable("SERVER_NAME") ?? "";


        // Use Entra-ID for Sql DB
		var credential = new AzureCliCredential();
		var accessToken = credential.GetToken(new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" })).Token;

        // Build the connection string 
        var connectionString = new SqlConnectionStringBuilder
        {
            DataSource = serverName, 
            InitialCatalog = databaseName, 
            Encrypt = true,
        }.ToString();
		

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string is missing.");
        }

		// create connection
        var sqlConnection = new SqlConnection(connectionString)
        {
            AccessToken = accessToken 
        };

		optionsBuilder.UseSqlServer(sqlConnection);

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

        // SectorPerformance 
        modelBuilder.Entity<SectorPerformance>()
            .HasKey(sp => new { sp.SectorName, sp.Date }); // Composite key for SectorPerformance

        modelBuilder.Entity<SectorPerformance>()
            .HasOne(sp => sp.Sector)
            .WithMany(s => s.Performances)
            .HasForeignKey(sp => sp.SectorName)
            .OnDelete(DeleteBehavior.Cascade);

            // Exchangerate
            modelBuilder.Entity<ExchangeRate>()
                .HasKey(er => new { er.Date, er.TargetCurrency });

            // InterestRate
			modelBuilder.Entity<InterestRate>()
                .HasKey(r => r.Id);
            
            // StockBalanceSeheet
            modelBuilder.Entity<StockBalanceSheet>()
                .HasKey(s => new { s.Ticker, s.FiscalYear });

            modelBuilder.Entity<StockBalanceSheet>()
                .HasOne(s => s.Stock)
                .WithMany(s => s.BalanceSheets)
                .HasForeignKey(s => s.Ticker)
                .OnDelete(DeleteBehavior.Cascade);
            
            // CashFlow Statement
            modelBuilder.Entity<CashFlowStatement>()
                .HasKey(s => new { s.Ticker, s.FiscalDateEnding });

            modelBuilder.Entity<CashFlowStatement>()
                .HasOne(s => s.Stock)
                .WithMany(s => s.CashFlowStatements)
                .HasForeignKey(s => s.Ticker)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Income Statement
            modelBuilder.Entity<IncomeStatement>()
                .HasKey(s => new { s.Ticker, s.FiscalDateEnding });

            modelBuilder.Entity<IncomeStatement>()
                .HasOne(s => s.Stock)
                .WithMany(s => s.IncomeStatements)
                .HasForeignKey(s => s.Ticker)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Earning Statement
            modelBuilder.Entity<StockEarningStatement>()
                .HasKey(s => new { s.Ticker, s.FiscalDateEnding });

            modelBuilder.Entity<StockEarningStatement>()
                .HasOne(s => s.Stock)
                .WithMany(s => s.EarningStatements)
                .HasForeignKey(s => s.Ticker)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Economic Indicator
            modelBuilder.Entity<EconomicIndicator>()
                .HasKey(ec => new { ec.Name });
            
            // Economic Data
            modelBuilder.Entity<EconomicData>()
                .HasKey(ed => new { ed.IndicatorName, ed.Date });
                
            modelBuilder.Entity<EconomicData>()
                .HasOne(ed => ed.Indicator)  
                .WithMany(ed => ed.DataPoints)
                .HasForeignKey(ed => ed.IndicatorName)
                .OnDelete(DeleteBehavior.Cascade);
			
			// Economic Indicator
 			modelBuilder.Entity<CommodityIndicator>()
                .HasKey(ec => new { ec.Name });

			// Comodities Data
            modelBuilder.Entity<CommodityData>()
                .HasKey(ed => new { ed.CommodityName, ed.Date });
                
            modelBuilder.Entity<CommodityData>()
                .HasOne(ed => ed.Commodity)  
                .WithMany(ed => ed.DataPoints)
                .HasForeignKey(ed => ed.CommodityName)
                .OnDelete(DeleteBehavior.Cascade);

			// Data Update Tracker
			modelBuilder.Entity<DataUpdateTracker>()
                .HasKey(d => d.DataType);
    }
}
}