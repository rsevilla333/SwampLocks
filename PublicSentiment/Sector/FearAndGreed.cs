//This class calculates the daily Fear and Greed Index for a specific sector.
//It loads articles and price data for each ticker in the sector, computes a sentiment score based on
//article relevance and stock momentum/strength, and outputs daily sentiment scores with optional smoothing and label classification.

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using SwampLocksDb.Data;

namespace PublicSentiment.Sector
{
    public class FearAndGreed
    {
        private readonly string connectionString;
        private readonly FinancialContext _context;
        private readonly List<string> _tickers;

        private Dictionary<DateTime, List<(decimal Sentiment, decimal Relevance)>>? _articleCache;
        private Dictionary<string, List<(DateTime Date, decimal ClosingPrice)>>? _priceCache;

        private const double rawBias = 0.15; // Constant deduction applied to final score

        public FearAndGreed(string connectionString, FinancialContext context, string sectorName)
        {
            this.connectionString = connectionString;
            this._context = context;

            // Get all tickers that belong to the given sector
            this._tickers = _context.Stocks
                .Where(s => s.Sector.Name == sectorName)
                .Select(s => s.Ticker)
                .ToList();
        }

        public void PreloadData(DateTime startDate, DateTime endDate)
        {
            // Load relevant articles into memory, grouped by date
            _articleCache = _context.Articles
                .Where(a => _tickers.Contains(a.Ticker) &&
                            a.Date.Date >= startDate.Date &&
                            a.Date.Date <= endDate.Date &&
                            a.RelevanceScore >= 0.1m)
                .AsEnumerable()
                .GroupBy(a => a.Date.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(a => (
                        a.SentimentScore,
                        Relevance: Math.Clamp(a.RelevanceScore, 0m, 1m)
                    )).ToList()
                );

            // Load stock prices for all tickers for the past year
            DateTime minDate = startDate.AddDays(-365);
            _priceCache = _context.StockDataEntries
                .Where(d => _tickers.Contains(d.Ticker) &&
                            d.Date >= minDate &&
                            d.Date <= endDate)
                .AsEnumerable()
                .GroupBy(d => d.Ticker)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(d => (d.Date, d.ClosingPrice))
                          .OrderBy(d => d.Date)
                          .ToList()
                );
        }

        private double CalculateRawScore(DateTime date)
        {
            if (_articleCache == null || _priceCache == null)
                throw new InvalidOperationException("PreloadData must be called before calculating sentiment.");

            double sentimentWeightedSum = 0;
            double relevanceSum = 0;

            // If we have no valid articles or not enough articles, reuse previous value
            if (!_articleCache.TryGetValue(date.Date, out var articles) || articles.Count < 10)
                return -1;

            foreach (var (sentiment, relevance) in articles)
            {
                sentimentWeightedSum += (double)(sentiment * relevance);
                relevanceSum += (double)relevance;
            }

            double sentimentComponent = (relevanceSum > 0) ? sentimentWeightedSum / relevanceSum : 0;
            double boundedSentiment = Math.Clamp(sentimentComponent, 0.05, 0.30);
            double normalizedSentiment = (boundedSentiment - 0.05) / (0.30 - 0.05);

            // Calculate 125-day momentum per ticker
            var momentumValues = _tickers.Select(ticker =>
            {
                if (_priceCache.TryGetValue(ticker, out var prices))
                {
                    var current = prices.FirstOrDefault(p => p.Date.Date == date.Date);
                    var past = prices.FirstOrDefault(p => p.Date.Date == date.AddDays(-125).Date);
                    if (current.ClosingPrice > 0 && past.ClosingPrice > 0)
                        return (double?)(current.ClosingPrice - past.ClosingPrice) / (double)past.ClosingPrice;
                }
                return null;
            })
            .Where(m => m.HasValue)
            .Select(m => m.Value)
            .ToList();

            double avgMomentum = momentumValues.Any() ? momentumValues.Average() : 0;
            double clampedMomentum = Math.Clamp(avgMomentum, -1, 1);
            double normalizedMomentum = (clampedMomentum + 1) / 2;

            // Calculate strength: % of stocks at 52-week high
            var strengthScores = _tickers.Select(ticker =>
            {
                if (_priceCache.TryGetValue(ticker, out var prices))
                {
                    var relevantPrices = prices.Where(p => p.Date <= date && p.Date >= date.AddDays(-365)).ToList();
                    if (relevantPrices.Count < 2)
                        return false;
                    var latest = relevantPrices.Last();
                    var maxPrice = relevantPrices.Max(p => p.ClosingPrice);
                    return latest.ClosingPrice == maxPrice;
                }
                return false;
            });

            double strengthRatio = strengthScores.Any() ? strengthScores.Count(b => b) / (double)strengthScores.Count() : 0;

            // Final weighted score using sentiment, momentum, and strength
            double finalScore = 0.7 * normalizedSentiment + 0.15 * normalizedMomentum + 0.15 * strengthRatio;
            finalScore = Math.Clamp(finalScore - rawBias, 0, 1);

            return finalScore;
        }

        public List<(DateTime Date, double Index, string Label)> GetSmoothedSectorSentiment(DateTime startDate, DateTime endDate)
        {
            var rawScores = new List<(DateTime Date, double Index)>();
            double lastValidScore = 50;

            // Generate raw score per day; reuse last valid if data is missing
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                double raw = CalculateRawScore(date);
                if (raw < 0)
                {
                    lastValidScore = Math.Max(lastValidScore, 5);
                    rawScores.Add((date, lastValidScore));
                }
                else
                {
                    double scaled = Math.Clamp(raw * 100, 0, 100);
                    lastValidScore = scaled;
                    rawScores.Add((date, scaled));
                }
            }

            var smoothed = new List<(DateTime, double, string)>();

            // Use a 3-day window to smooth scores and assign sentiment label
            for (int i = 0; i < rawScores.Count; i++)
            {
                var window = rawScores.Skip(Math.Max(0, i - 1)).Take(3).Select(x => x.Index).ToList();
                double average = window.Average();
                smoothed.Add((rawScores[i].Date, average, InterpretIndex(average)));
            }

            return smoothed;
        }

        // Translate a numeric index into a sentiment category
        public string InterpretIndex(double index)
        {
            if (index < 30) return "Extreme Fear";
            if (index < 45) return "Fear";
            if (index < 55) return "Neutral";
            if (index < 70) return "Greed";
            return "Extreme Greed";
        }

        
        public void DisplayIndex(double index)
        {
            Console.WriteLine($"\nSector Greed and Fear Index: {index:F2}");
            string interpretation = InterpretIndex(index);
            switch (interpretation)
            {
                case "Extreme Fear": Console.WriteLine("Sector is in Extreme Fear."); break;
                case "Fear": Console.WriteLine("Sector is in Fear."); break;
                case "Neutral": Console.WriteLine("Sector sentiment is Neutral."); break;
                case "Greed": Console.WriteLine("Sector is in Greed."); break;
                case "Extreme Greed": Console.WriteLine("Sector is in Extreme Greed."); break;
            }
        }
    }
}

