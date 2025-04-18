using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ScottPlot;
using SwampLocksDb.Data;

namespace PublicSentiment.Sector.Visualize.Graph
{
    public class SectorSentimentGraph
    {
        private readonly string _connectionString;
        private readonly FinancialContext _context;

        public SectorSentimentGraph(string connectionString)
        {
            _connectionString = connectionString;

            var optionsBuilder = new DbContextOptionsBuilder<FinancialContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            _context = new FinancialContext(optionsBuilder.Options);
        }

        public void PlotSectorSentiment(string sectorName, int daysBack = 30)
        {
            Console.WriteLine($"\n📈 Generating graph for sector: {sectorName}");

            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-daysBack);

            var sentimentByDate = new Dictionary<DateTime, double>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var tickers = _context.Stocks
                    .Where(s => s.Sector.Name == sectorName)
                    .Select(s => s.Ticker)
                    .ToList();

                double totalSentiment = 0;
                int count = 0;

                foreach (var ticker in tickers)
                {
                    var scores = _context.Articles
                        .Where(a => a.Ticker == ticker && a.Date.Date == date.Date)
                        .Select(a => a.SentimentScore)
                        .ToList();

                    if (scores.Any())
                    {
                        totalSentiment += scores.Sum(x => (double)x); // 👈 Fix decimal -> double
                        count += scores.Count;
                    }
                }

                double avg = (count > 0) ? totalSentiment / count : 0;
                sentimentByDate[date] = avg;
            }

            var plt = new ScottPlot.Plot();

            double[] xs = sentimentByDate.Keys.Select(x => x.ToOADate()).ToArray();
            double[] ys = sentimentByDate.Values.ToArray();

            plt.Add.Scatter(xs, ys);
            plt.Axes.DateTimeTicksBottom();
            plt.Title($"{sectorName} - Sentiment Over Last {daysBack} Days");
            plt.XLabel("Date");
            plt.YLabel("Sentiment Index");

            string fileName = $"Visualize/Graph/{sectorName.Replace(" ", "_")}_SentimentGraph.png";
            plt.SavePng(fileName, 800, 450); // 👈 ScottPlot 5 API

            Console.WriteLine($"✅ Saved: {fileName}");
        }
    }
}
