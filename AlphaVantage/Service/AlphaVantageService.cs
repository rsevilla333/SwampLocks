using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SwampLocksDb.Data;
using SwampLocksDb.Models;
using SwampLocks.AlphaVantage.Client;
using SwampLocksDb.Models;
using SwampLocksDb.Data;

namespace SwampLocks.AlphaVantage.Service
{
    public class AlphaVantageService
    {
        private readonly FinancialContext _context;
        private readonly AlphaVantageClient _client;

        public AlphaVantageService(FinancialContext context, AlphaVantageClient client)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public void PopulateExchangeRates()
        {
            getExchangeRatesFor("EUR");
            getExchangeRatesFor("JPY");
            getExchangeRatesFor("BTC");
            getExchangeRatesFor("CAD");
        }

        public void PopulateSectors()
        {
            PopulateSector("Communication Services", "XLC");  // Communication Services Sector
            PopulateSector("Consumer Discretionary", "XLY");  // Consumer Discretionary Sector  
            PopulateSector("Consumer Staples", "XLP");  // Consumer Staples Sector  
            PopulateSector("Energy", "XLE");  // Energy Sector  
            PopulateSector("Financials", "XLF");  // Financials Sector  
            PopulateSector("Healthcare", "XLV");  // Healthcare Sector  
            PopulateSector("Industrials", "XLI");  // Industrials Sector  
            PopulateSector("Information Technology", "XLK");  // Information Technology Sector  
            PopulateSector("Materials", "XLB");  // Materials Sector  
            PopulateSector("Real Estate", "XLRE");  // Real Estate Sector  
            PopulateSector("Utilities", "XLU");  // Utilities Sector  
        }

        public bool PopulateSector(string sectorName, string etf)
        {
            var sectorStocksTickers = _client.GetStocksFromETF(etf);

            foreach (var ticker in sectorStocksTickers)
            {
                AddStock(ticker, sectorName);
            }
            
            Console.WriteLine($"Populated {sectorName} sector");
            return true;
        }
        
        public bool FetchAndStoreAllArticlesByStock(DateTime from, DateTime to)
        {
            // Retrieve all stocks from the database
            var stocks = _context.Stocks.ToList();
            
            if (stocks.Any())
            {
                foreach (var stock in stocks)
                {
                    FetchAndStoreArticlesByStock(stock.Ticker, from, to);
                }
            }
            else
            {
                Console.WriteLine("No stocks found in the database.");
            }
            
            return true;
        }
        

        public bool FetchAndStoreArticlesByStock(string ticker, DateTime from, DateTime to)
        {
            List<Tuple<DateTime,string, Decimal>> articles = _client.GetNewsSentimentByStock(ticker, from, to);

            foreach (var article in articles)
            {
                DateTime articleDate = article.Item1.Date; 
                string articleTitle = article.Item2;  
                decimal sentimentScore = article.Item3; 
                
                var newsEntry = new Article
                {
                    Ticker = ticker,
                    ArticleName = articleTitle,
                    Date = articleDate,
                    SentimentScore = sentimentScore
                };
                
                // Check if article already exists (avoid duplicates)
                if (!_context.Articles.Any(a => a.Id == newsEntry.Id))
                {
                    _context.Articles.Add(newsEntry);
                    Console.WriteLine($"Added: {newsEntry.ArticleName} (Date: {articleDate:yyyy-MM-dd}, sentiment: {sentimentScore})");
                }
            }
            
            _context.SaveChanges();
            
            return true;
        }

        public bool AddStock(string ticker, string sectorName)
        {
            var sector = _context.Sectors.FirstOrDefault(s => s.Name == sectorName);
            if (sector == null)
            {
                Console.WriteLine("Sector not found");
                return false;
            }
            
            var stock = _context.Stocks.FirstOrDefault(s => s.Ticker == ticker);
            
            Console.WriteLine($"Adding stock: {ticker} to sector: {sectorName}");
            
            if (stock == null)
            {
                // Create stock if it does not exist
                stock = new Stock
                {
                    Ticker = ticker,
                    SectorName = sectorName,
                };
                _context.Stocks.Add(stock);
            } else return false;
            
            _context.SaveChanges();
            Console.WriteLine($"Successfully added stock, ticker: {ticker}, sector: {sectorName}");
            return true;
        }

        private bool getExchangeRatesFor(string symbol)
        {
            List<Tuple<DateTime, Decimal>> exRates = _client.GetExchangeRateDaily("USD", symbol);
            
            var newRates = new List<ExchangeRate>();
            
            foreach (var rate in exRates)
            {
                // Check if the exchange rate already exists in the database
                bool exists = _context.ExchangeRates
                    .Any(r => r.Date == rate.Item1.Date && r.TargetCurrency == symbol);

                if (!exists)
                {
                    // Add new exchange rate
                    newRates.Add(new ExchangeRate
                    {
                        Date = rate.Item1.Date, 
                        TargetCurrency = symbol,
                        Rate = rate.Item2
                    });
                }
            }
            
            if (newRates.Any())
            {
                _context.ExchangeRates.AddRange(newRates);
                _context.SaveChanges();
                Console.WriteLine($"Inserted {newRates.Count} new exchange rates for {symbol}.");
                return true; // Indicates new data was added
            }
            
            Console.WriteLine($"No new exchange rates for {symbol}, all data already exists.");
            return false; // No new data added
        }
    }
}
