// This program generates sentiment logs for each individual sector over the past 730 days. It loads sector-wise data,
// computes a sentiment score per day using FearAndGreed, and saves each sector’s results in a separate CSV file inside a SectorLogs/ directory.


using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using SwampLocksDb.Data;
using PublicSentiment.Sector;

class Program
{
    static void Main(string[] args)
    {
        // Load .env file to access the DB connection string
        Env.Load();

        string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("CONNECTION_STRING not found in environment variables.");
            return;
        }

        // Setup EF database context with retry options
        var optionsBuilder = new DbContextOptionsBuilder<FinancialContext>();
        optionsBuilder.UseSqlServer(connectionString, opt => opt.EnableRetryOnFailure());
        var context = new FinancialContext(optionsBuilder.Options);

        Console.WriteLine("Database context initialized.");
        Console.WriteLine($"Total stocks in DB: {context.Stocks.Count()}");

        // Get list of sector names from DB
        var sectors = context.Sectors.Select(s => s.Name).ToList();

        // Ensure output directory exists
        if (!Directory.Exists("SectorLogs"))
        {
            Directory.CreateDirectory("SectorLogs");
        }

        // Set analysis date range (past 730 days)
        DateTime today = DateTime.Today;
        int daysBack = 730;
        DateTime startDate = today.AddDays(-(daysBack - 1));

        // Loop through all sectors and log their sentiment history
        foreach (var sectorName in sectors)
        {
            Console.WriteLine($"\nProcessing sector: {sectorName}");
            try
            {
                // Create sentiment object for this sector
                var fg = new FearAndGreed(connectionString, context, sectorName);
                fg.PreloadData(startDate, today);

                // Clean sector name for file path (removes spaces and symbols)
                string safeName = string.Concat(sectorName.Where(char.IsLetterOrDigit));
                string fileName = Path.Combine("SectorLogs", $"SectorSentiment_{safeName}.csv");

                using var writer = new StreamWriter(fileName);
                writer.WriteLine("Date,Sentiment,Label");

                // Retrieve daily sentiment data
                var sentimentResults = fg.GetSmoothedSectorSentiment(startDate, today);
                foreach (var (date, index, label) in sentimentResults)
                {
                    string line = $"{date:yyyy-MM-dd},{index:F2},{label}";
                    Console.WriteLine(line);
                    writer.WriteLine(line);
                    writer.Flush();
                }

                Console.WriteLine($"Finished logging for {sectorName}. Saved to {fileName}\n");
            }
            catch (Exception ex)
            {
                // If one sector fails, continue with the rest
                Console.WriteLine($"Error processing sector {sectorName}: {ex.Message}");
            }
        }

        Console.WriteLine("All sector logs generated in SectorLogs/");
    }
}
