//This program calculates the overall market-wide sentiment index over the past 730 days. 
//It loads sentiment data for all sectors, computes a weighted score per day,
//applies boost/deduction logic (e.g., momentum, optimism), and logs results into a CSV file.

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
        // Load environment variables from .env file
        Env.Load();

        string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")!;
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("CONNECTION_STRING not found in environment variables.");
            return;
        }

        // Set up Entity Framework DB context with retry policy
        var optionsBuilder = new DbContextOptionsBuilder<FinancialContext>();
        optionsBuilder.UseSqlServer(connectionString, opt => opt.EnableRetryOnFailure());
        var context = new FinancialContext(optionsBuilder.Options);

        Console.WriteLine("Database context initialized. Calculating Market-Wide Sentiment...\n");

        // Define date range for analysis (last 730 days)
        DateTime today = DateTime.Today;
        int daysBack = 730;
        DateTime startDate = today.AddDays(-(daysBack - 1));

        // Sector weights used for computing a weighted market sentiment
        var sectorWeights = new Dictionary<string, double>
        {
            ["Information Technology"] = 0.30,
            ["Health Care"] = 0.12,
            ["Financials"] = 0.11,
            ["Consumer Discretionary"] = 0.11,
            ["Communication Services"] = 0.09,
            ["Industrials"] = 0.08,
            ["Consumer Staples"] = 0.06,
            ["Energy"] = 0.05,
            ["Utilities"] = 0.03,
            ["Real Estate"] = 0.02,
            ["Materials"] = 0.03,
        };

        // Preload data for all sectors and store FearAndGreed instances
        var fgBySector = new Dictionary<string, FearAndGreed>();
        foreach (var kvp in sectorWeights)
        {
            var fg = new FearAndGreed(connectionString, context, kvp.Key);
            fg.PreloadData(startDate, today);
            fgBySector[kvp.Key] = fg;
        }

        // Prepare output CSV
        string fileName = "MarketSentiment.csv";
        using StreamWriter writer = new StreamWriter(fileName);
        writer.WriteLine("Date,Sentiment,Label");

        List<double> prevScores = new List<double>();

        for (int i = 0; i < daysBack; i++)
        {
            DateTime date = today.AddDays(-i);
            double totalWeightedScore = 0;
            double totalWeight = 0;
            int risingSectors = 0;
            int fearSectors = 0;

            // Aggregate weighted score across sectors
            foreach (var kvp in sectorWeights)
            {
                var fg = fgBySector[kvp.Key];
                var score = fg.GetSmoothedSectorSentiment(date, date).FirstOrDefault().Index;

                if (score > 50) risingSectors++;
                if (score < 40) fearSectors++;

                totalWeightedScore += score * kvp.Value;
                totalWeight += kvp.Value;
            }

            double scoreToday = totalWeight > 0 ? totalWeightedScore / totalWeight : 50;

            // Apply momentum greed boost if sentiment is rising for 3 consecutive days
            if (prevScores.Count >= 3 && prevScores.All(s => scoreToday > s))
            {
                double avgPrev = prevScores.Average();
                double momentumFactor = (scoreToday - avgPrev) / 100;
                double boost = Math.Min(momentumFactor, 0.10);
                scoreToday *= 1 + boost;
            }

            // Extra boost if current score and two previous are rising above 55
            if (scoreToday > 55 && prevScores.Count >= 2 && scoreToday > prevScores[0] && prevScores[0] > prevScores[1])
            {
                scoreToday *= 1.08;
            }

            // Small bump if over 70% of sectors are rising
            if (risingSectors >= (int)(sectorWeights.Count * 0.7))
            {
                scoreToday += 2.5;
            }

            // Deduction if more than half of sectors are under 40
            if (fearSectors >= (int)(sectorWeights.Count * 0.5))
            {
                scoreToday -= 2.5;
            }

            // Deduction if current score is below 45 and last 3 days were also low
            if (scoreToday < 45 && prevScores.Count >= 3 && prevScores.Take(3).All(s => s < 50))
            {
                scoreToday -= 2.5;
            }

            // Clamp score to be within 0 and 100
            scoreToday = Math.Clamp(scoreToday, 0, 100);

            // Store the score and update moving window
            prevScores.Insert(0, scoreToday);
            if (prevScores.Count > 5)
                prevScores.RemoveAt(5);

            // Use any sector's InterpretIndex to classify the final label
            string label = fgBySector.Values.First().InterpretIndex(scoreToday);
            string line = $"{date:yyyy-MM-dd},{scoreToday:F2},{label}";

            Console.WriteLine(line);
            writer.WriteLine(line);
            writer.Flush();
        }

        Console.WriteLine($"\nMarket Sentiment log complete. File saved as {fileName}\n");
    }
}
