using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using SwampLocksDb.Data;
using SwampLocksDb.Models;
using System.Text.Json;
using SwampLocks.EmailSevice; 

namespace SwampLocksAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinancialsController : ControllerBase
    {
        private readonly FinancialContext _context;
        private readonly HttpClient _httpClient;
        private readonly EmailNotificationService _emailService;

        private readonly string _alphaKey;

        public FinancialsController(FinancialContext context, HttpClient httpClient, EmailNotificationService emailService)
        {
             Env.Load();
             
            _context = context;
            _httpClient = httpClient;
            _alphaKey = Environment.GetEnvironmentVariable("ALPHA_VANTAGE");
            _emailService = emailService;
            
        }

        [HttpGet("ping")]

        public async Task<ActionResult<string>> PingTest()
        {
            Stock stock = new();
            stock.Ticker = "AAPL";
            return Ok("test with auto");
        }
        
        [HttpGet("login/{userEmail}/{name}")]
        public async Task<ActionResult> Login(string userEmail, string name)
        {
            // Check if the user exists in the database/context
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (existingUser != null)
            {
                // If the user exists, don't send the email and return a success message
                return Ok(new { message = "User already exists, no email sent." });
            }

            // If the user does not exist, create the new user in the database
            var newUser = new User
            {
                Email = userEmail,
                FullName = name,
                DateCreated = DateTime.UtcNow
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            // Compose the welcome email subject and body
            var subject = "Welcome to SwampLocks!";
            
            var body = @"
            Hello, 

            Welcome to SwampLocks!

            We are excited to have you join our community of financial enthusiasts and professionals. SwampLocks is designed to help you optimize and predict your investment portfolio with the power of machine learning. With our tool, you can:

            - Optimize your investment strategies
            - Leverage cutting-edge machine learning algorithms for financial predictions
            - Get insights from real-time financial data
            - And much more!

            We are committed to providing you with the best tools to help you make informed financial decisions. Your account has been successfully created, and you can now start exploring SwampLocks to take your financial planning to the next level.

            If you have any questions or need assistance, feel free to reach out to us at support@swamplocks.com. 

            Best regards, 
            The SwampLocks Team -> (Rafael, Chandler, Deep, Mathew, Andres) ";

            // Send the welcome email
            await _emailService.SendEmailNotification(userEmail, subject, body);

            return Ok(new { message = "Welcome email sent successfully and user created." });
        }

        [HttpGet("stocks")]
        public async Task<ActionResult<List<Stock>>> GetAllStocks()
        {
            List<Stock> stocks = await _context.Stocks.ToListAsync();

            if (stocks.Count == 0)
            {
                return NotFound();
            }

            return Ok(stocks);
        }

        [HttpGet("sectors")]
        public async Task<ActionResult<List<Stock>>> GetAllSectors()
        {
            List<Sector> sectors = await _context.Sectors.ToListAsync();

            if (sectors.Count == 0)
            {
                return NotFound();
            }

            return Ok(sectors);
        }
        
        [HttpGet("top-marketcap")]
        public ActionResult<List<StockData>> GetTopMarketCapStocks(
            [FromQuery] DateTime? date,
            [FromQuery] int count = 20,
            [FromQuery] string sectorName = null)
        {
            
            DateTime targetDate;

            if (date.HasValue)
            {
                targetDate = date.Value.Date;
            }
            else
            {
                var latestEntry = _context.StockDataEntries
                    .OrderByDescending(sd => sd.Date)
                    .FirstOrDefault();

                if (latestEntry == null)
                {
                    return NotFound("No stock data available in the database.");
                }

                targetDate = latestEntry.Date.Date;
            }
            
            var query = _context.StockDataEntries
                .Where(sd => sd.Date.Date == targetDate && sd.MarketCap > 0)
                .Include(sd => sd.Stock)
                .AsQueryable();

            if (!string.IsNullOrEmpty(sectorName))
            {
                query = query.Where(sd => sd.Stock.SectorName == sectorName);
            }

            var topStocks = query
                .OrderByDescending(sd => sd.MarketCap)
                .Take(count)
                .ToList();

            if (topStocks.Count == 0)
            {
                return NotFound("No stock data found for the given parameters.");
            }

            return Ok(topStocks);
        }
        
        [HttpGet("top-marketcap-with-change")]
        public ActionResult<List<StockWithChangeDto>> GetTopMarketCapWithChange(
            [FromQuery] DateTime? date,
            [FromQuery] int count = 10,
            [FromQuery] string sectorName = null)
        {
            DateTime latestDate;

            if (date.HasValue)
            {
                latestDate = date.Value.Date;
            }
            else
            { 
                latestDate = _context.StockDataEntries
                    .OrderByDescending(sd => sd.Date)
                    .Select(sd => sd.Date)
                    .FirstOrDefault();
            }
            
            if (latestDate == default)
                return NotFound("No stock data available.");

            var previousDate = _context.StockDataEntries
                .Where(sd => sd.Date < latestDate)
                .OrderByDescending(sd => sd.Date)
                .Select(sd => sd.Date)
                .FirstOrDefault();

            if (previousDate == default)
            {
                return NotFound("No previous trading day data found.");
            }

            var latestData = _context.StockDataEntries
                .Where(sd => sd.Date.Date == latestDate.Date && sd.MarketCap > 0)
                .Include(sd => sd.Stock)
                .AsQueryable();

            if (!string.IsNullOrEmpty(sectorName))
                latestData = latestData.Where(sd => sd.Stock.SectorName == sectorName);

            var topStocks = latestData
                .OrderByDescending(sd => sd.MarketCap)
                .Take(count)
                .ToList();

            var tickers = topStocks.Select(sd => sd.Ticker).ToList();

            var previousData = _context.StockDataEntries
                .Where(sd => tickers.Contains(sd.Ticker) && sd.Date.Date == previousDate.Date)
                .ToList()
                .ToDictionary(sd => sd.Ticker, sd => sd.ClosingPrice);

            var result = topStocks.Select(sd =>
            {
                decimal change = 0;
                if (previousData.TryGetValue(sd.Ticker, out var prevPrice) && prevPrice > 0)
                {
                    change = ((sd.ClosingPrice - prevPrice) / prevPrice) * 100;
                }

                return new StockWithChangeDto
                {
                    Symbol = sd.Ticker,
                    MarketCap = sd.MarketCap,
                    Change = Math.Round(change, 2)
                };
            }).ToList();

            return Ok(result);
        }
        
        [HttpGet("top-movers")]
        public ActionResult<List<StockWithChangeDto>> GetTopMovers(
            [FromQuery] DateTime? date,
            [FromQuery] int count = 10,
            [FromQuery] string sectorName = null)
        {
            DateTime latestDate;

            if (date.HasValue)
            {
                latestDate = date.Value.Date;
            }
            else
            {
                latestDate = _context.StockDataEntries
                    .OrderByDescending(sd => sd.Date)
                    .Select(sd => sd.Date)
                    .FirstOrDefault();
            }

            if (latestDate == default)
                return NotFound("No stock data available.");

            var previousDate = _context.StockDataEntries
                .Where(sd => sd.Date < latestDate)
                .OrderByDescending(sd => sd.Date)
                .Select(sd => sd.Date)
                .FirstOrDefault();

            if (previousDate == default)
                return NotFound("No previous trading day data found.");

            var latestDataQuery = _context.StockDataEntries
                .Where(sd => sd.Date == latestDate && sd.MarketCap > 0)
                .Include(sd => sd.Stock)
                .AsQueryable();

            if (!string.IsNullOrEmpty(sectorName))
                latestDataQuery = latestDataQuery.Where(sd => sd.Stock.SectorName == sectorName);

            var latestData = latestDataQuery.ToList();

            var tickers = latestData.Select(sd => sd.Ticker).ToList();

            var previousDataDict = _context.StockDataEntries
                .Where(sd => tickers.Contains(sd.Ticker) && sd.Date == previousDate)
                .ToList()
                .ToDictionary(sd => sd.Ticker, sd => sd.ClosingPrice);

            var result = latestData.Select(sd =>
            {
                decimal change = 0;
                if (previousDataDict.TryGetValue(sd.Ticker, out var prevPrice) && prevPrice > 0)
                {
                    change = ((sd.ClosingPrice - prevPrice) / prevPrice) * 100;
                }

                return new StockWithChangeDto
                {
                    Symbol = sd.Ticker,
                    MarketCap = sd.MarketCap,
                    Change = Math.Round(change, 2)
                };
            })
            .OrderByDescending(sd => Math.Abs(sd.Change)) 
            .Take(count)
            .ToList();

            return Ok(result);
        }
        
        // MOCK DATA FOR ML MODEL
        [HttpGet("sector/{name}/performance_ml")]
        public ActionResult<decimal> GetSectorPerformanceML(string name)
        {
            // model logic would go here 
            var random = new Random();
            decimal mockScore = (decimal)random.NextDouble(); 

            return Ok(Math.Round(mockScore, 4)); 
        }
        

        [HttpGet("stocks/{ticker}/data")]
        public async Task<ActionResult<List<StockData>>> GetStockData(string ticker)
        {
            List<StockData> stockData = await _context
                .StockDataEntries
                .Where(data => data.Ticker == ticker)
                .ToListAsync();

            if (stockData.Count == 0)
            {
                return NotFound();
            }

            return Ok(stockData);
        }
        
        [HttpGet("stocks/{ticker}/todays_data")]
        public async Task<ActionResult<List<StockData>>> GetTodaysStockData(string ticker)
        {
            if (string.IsNullOrEmpty(_alphaKey))
            {
                return StatusCode(500, "Cant communicate with ALPHA API");
            }

            string interval = "1min";
            string url = $"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol={ticker}&interval={interval}&apikey={_alphaKey}";
            
            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var jsonResponse = JsonDocument.Parse(response);

                // Parse the Time Series data
                var timeSeries = jsonResponse.RootElement.GetProperty($"Time Series ({interval})");

                var stockData = new List<StockData>();

                foreach (var item in timeSeries.EnumerateObject())
                {
                    var date = DateTime.Parse(item.Name); // Parsing date from the response
                    var data = item.Value;

                    var stock = new StockData
                    {
                        Ticker = ticker,
                        Date = date,
                        ClosingPrice = decimal.Parse(data.GetProperty("4. close").GetString()),
                        MarketCap = 0, 
                        PublicSentiment = 0, 
                    };

                    stockData.Add(stock);
                }

                return Ok(stockData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching data: {ex.Message}");
            }
        }
        
        [HttpGet("stocks/{ticker}/filtered_data")]
        public async Task<ActionResult<List<StockData>>> GetFilteredStockData(string ticker)
        {
            List<StockData> stockData = await _context
                .StockDataEntries
                .Where(data => data.Ticker == ticker)
                .OrderByDescending(data => data.Date) 
                .ToListAsync();
            
            if (stockData.Count == 0)
            {
                return NotFound();
            }

            
            List<StockSplit> stockSplits = await _context
                .StockSplits
                .Where(split => split.Ticker == ticker)
                .OrderByDescending(split => split.EffectiveDate) 
                .ToListAsync();
            
            if (stockSplits.Count == 0)
            {
                Console.WriteLine($"No stock splits found for ticker: {ticker}");
                return Ok(stockData);
            }

            decimal cumulativeSplitFactor = 1;
            var stockDataList = stockData;
            foreach (var data in stockData)
            {
                var splitsToApply = stockSplits;
                bool skipSlpit = false;
                foreach (var split in splitsToApply)
                {
                     
                    if (data.Date <= split.EffectiveDate)
                    {
                        if(data.Date == split.EffectiveDate)
                        {
                            data.ClosingPrice /= cumulativeSplitFactor;
                            skipSlpit = true;
                        }
                        
                        cumulativeSplitFactor *= split.SplitFactor;
                        stockSplits.Remove(split);
                        
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                
                if(!skipSlpit)
                {
                    data.ClosingPrice /= cumulativeSplitFactor;
                }
            }

            stockData.Reverse();
            return Ok(stockData);
        }

        
        [HttpGet("stocks/{ticker}/exists")]
        public async Task<ActionResult<bool>> CheckIfStockExists(string ticker)
        {
            // Query your database to check if a stock with this ticker exists
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.Ticker == ticker);

            if (stock == null)
            {
                // If stock doesn't exist
                return NotFound(new { message = $"Stock with ticker {ticker} not found." });
            }

            // If stock exists
            return Ok(true);
        }
        
        [HttpGet("stocks/autocomplete")]
        public async Task<ActionResult<IEnumerable<string>>> GetMatchingStockTickers(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest(new { message = "Query is required." });
            }
            
            var matchingStocks = await _context.Stocks
                .Where(s => s.Ticker.StartsWith(query.ToUpper())) // case-insensitivity
                .Select(s => s.Ticker)
                .Take(10) // # of suggestions
                .ToListAsync();

            // If no matching stocks are found
            if (matchingStocks.Count == 0)
            {
                return NotFound(new { message = "No matching stock tickers found." });
            }

            // Return the list of matching stock tickers
            return Ok(matchingStocks);
        }


        [HttpGet("stocks/{ticker}/data/{timeframe}")]
        public async Task<ActionResult<List<StockData>>> GetStockData(string ticker, string timeframe)
        {
            DateTime endDate = DateTime.UtcNow; 
            DateTime startDate;

            switch (timeframe.ToLower())
            {
                case "1w":
                    startDate = endDate.AddDays(-7);
                    break;
                case "6m":
                    startDate = endDate.AddMonths(-6);
                    break;
                case "1y":
                    startDate = endDate.AddYears(-1);
                    break;
                case "5y":
                    startDate = endDate.AddYears(-5);
                    break;
                case "ytd":
                    startDate = new DateTime(endDate.Year, 1, 1); // Start of the current year
                    break;
                default:
                    return BadRequest(new { message = "Invalid timeframe. Use '1w', '6m', '1y', '5y', or 'ytd'." });
            }

            var stockData = await _context.StockDataEntries
                .Where(data => data.Ticker == ticker && data.Date >= startDate && data.Date <= endDate)
                .OrderBy(data => data.Date)
                .ToListAsync();

            if (!stockData.Any())
            {
                return NotFound(new { message = $"No stock data found for {ticker} in the {timeframe} timeframe." });
            }

            return Ok(stockData);
        }

        [HttpGet("stocks/{ticker}/articles")]
        public async Task<ActionResult<List<Article>>> GetStockArticles(string ticker)
        {
            List<Article> articles = await _context
                .Articles
                .Where(data => data.Ticker == ticker)
                .ToListAsync();
            
            return Ok(articles);
        }
        
        [HttpGet("stocks/articles/all")]
        public async Task<ActionResult<List<Article>>> GetAllStockArticles()
        {
            List<Article> articles = await _context
                .Articles
                .Where(data => data.Date >= DateTime.Now.AddDays(-30))
                .ToListAsync();
            
            return Ok(articles);
        }


        
        [HttpGet("commodities/{commodityName}")]
        public async Task<ActionResult<List<CommodityData>>> GetCommodityData(string commodityName)
        {
            List<CommodityData> commodityData = await _context
                .CommodityDataPoints
                .Where(commodity => commodity.CommodityName == commodityName)
                .ToListAsync();

            if (commodityData.Count == 0)
            {
                return NotFound();
            }

            return Ok(commodityData);
        }
        
        [HttpGet("commodities/indicators")]
        public async Task<ActionResult<List<CommodityIndicator>>> GetCommodityIndicators()
        {
            List<CommodityIndicator> indicators = await _context
                .Commodities
                .ToListAsync();

            if (indicators.Count == 0)
            {
                return NotFound();
            }

            return Ok(indicators);
        }
        
        [HttpGet("economic_data/{indicatorName}")]
        public async Task<ActionResult<List<EconomicData>>> GetEconomicData(string indicatorName)
        {
            List<EconomicData> ecoData = await _context
                .EconomicDataPoints
                .Where(ecoPoint => ecoPoint.IndicatorName == indicatorName)
                .ToListAsync();

            if (ecoData.Count == 0)
            {
                return NotFound();
            }

            return Ok(ecoData);
        }
        
        [HttpGet("economic_data/indicators")]
        public async Task<ActionResult<List<EconomicIndicator>>> GetEconomicIndicators()
        {
            List<EconomicIndicator> indicators = await _context
                .EconomicIndicators
                .ToListAsync();

            if (indicators.Count == 0)
            {
                return NotFound();
            }

            return Ok(indicators);
        }

        [HttpGet("balancesheets/{ticker}")]
        public async Task<ActionResult<List<StockBalanceSheet>>> GetBalanceSheets(string ticker)
        {
            List<StockBalanceSheet> balanceSheets = await _context
                .StockBalanceSheets
                .Where(balanceSheet => balanceSheet.Ticker == ticker)
                .ToListAsync();

            if (balanceSheets.Count == 0)
            {
                return NotFound();
            }

            return Ok(balanceSheets);
        }

        [HttpGet("cashflowstatements/{ticker}")]
        public async Task<ActionResult<List<CashFlowStatement>>> GetCashFlowStatements(string ticker)
        {
            List<CashFlowStatement> cashFlowStatements = await _context
                .CashFlowStatements
                .Where(statement => statement.Ticker == ticker)
                .ToListAsync();

            if (cashFlowStatements.Count == 0)
            {
                return NotFound();
            }

            return Ok(cashFlowStatements);
        }

        [HttpGet("earnings/{ticker}")]
        public async Task<ActionResult<List<StockEarningStatement>>> GetEarnings(string ticker)
        {
            List<StockEarningStatement> earnings = await _context
                .StockEarnings
                .Where(earnings => earnings.Ticker == ticker)
                .ToListAsync();

            if (earnings.Count == 0)
            {
                return NotFound();
            }

            return Ok(earnings);
        }

        [HttpGet("incomestatements/{ticker}")]
        public async Task<ActionResult<List<IncomeStatement>>> GetIncomeStatements(string ticker)
        {
            List<IncomeStatement> incomeStatements = await _context
                .IncomeStatements
                .Where(incomeStatement => incomeStatement.Ticker == ticker)
                .ToListAsync();

            if (incomeStatements.Count == 0)
            {
                return NotFound();
            }

            return Ok(incomeStatements);
        }

        [HttpGet("sectorperformance/{sector}")]
        public async Task<ActionResult<List<SectorPerformance>>> GetSectorPerformance(string sector)
        {
            List<SectorPerformance> sectorPerformances = await _context
                .SectorPerformances
                .Where(sectorPerformance => sectorPerformance.SectorName == sector)
                .ToListAsync();

            if (sectorPerformances.Count == 0)
            {
                return NotFound();
            }

            return Ok(sectorPerformances);
        }

		[HttpGet("sectorstocks/{sectorName}")]
		public async Task<ActionResult<List<Stock>>> GetStocksFromSector(string sectorName)
		{
    		var stocks = await _context.Stocks
        		.Where(s => s.SectorName == sectorName)
        		.ToListAsync();

    		if (stocks.Count == 0)
    		{
        		return NotFound();
    		}

    		return Ok(stocks);
		}  
        
        [HttpGet("ex_rates")]
        public async Task<ActionResult<List<ExchangeRate>>> GetExRates()
        {
            List<ExchangeRate> exRates = await _context
                .ExchangeRates
                .ToListAsync();

            if (exRates.Count == 0)
            {
                return NotFound();
            }

            return Ok(exRates);
        }
        
        [HttpGet("top_movers")]
        public async Task<ActionResult<List<MarketMovers>>> GetTopMovers()
        {
            if (string.IsNullOrEmpty(_alphaKey))
            {
                return StatusCode(500, "Cant communicate with ALPHA API");
            }
            
            string url = $"https://www.alphavantage.co/query?function=TOP_GAINERS_LOSERS&apikey={_alphaKey}";
            
            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var jsonResponse = JsonDocument.Parse(response);

                var movers = jsonResponse.RootElement.GetProperty("top_gainers").EnumerateArray()
                    .Concat(jsonResponse.RootElement.GetProperty("top_losers").EnumerateArray())
                    .Select(m => new MarketMovers
                    {
                        Ticker = m.GetProperty("ticker").GetString(),
                        Price = decimal.Parse(m.GetProperty("price").GetString()),
                        Change = decimal.Parse(m.GetProperty("change_amount").GetString()),
                        ChangePercent = decimal.Parse(m.GetProperty("change_percentage").GetString().TrimEnd('%')),
                        Volume = long.Parse(m.GetProperty("volume").GetString())
                    }).ToList();

                return Ok(movers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching data: {ex.Message}");
            }
        }
        
        
        [HttpGet("article_preview/{url}")]
        public async Task<IActionResult> GetPreview([FromQuery] string url)
        {
            if (string.IsNullOrEmpty(url)) return BadRequest("URL is required");

            try
            {
                var response = await _httpClient.GetStringAsync($"https://api.linkpreview.net/?key=YOUR_API_KEY&q={url}");
                return Ok(response);
            }
            catch
            {
                return BadRequest("Failed to fetch link preview");
            }
        }

        [HttpPatch("stocks/data/{id}")]
        public async Task<IActionResult> UpdateMarketCap(string id, [FromBody] JsonPatchDocument<StockData> stockDataPatch)
        {
            if (stockDataPatch == null)
            {
                return BadRequest(new { message = "Patch data not found" });
            }
            else
            {
                string ticker = id.Split("_")[0];
                string dateString = id.Split("_")[1];

                DateTime date = DateTime.ParseExact(dateString, "yyyyMMdd", null);

                StockData? dataEntry = await _context
                    .StockDataEntries
                    .SingleOrDefaultAsync(data => data.Ticker == ticker && data.Date == date);

                if (dataEntry == null)
                {
                    return NotFound();
                }

                stockDataPatch.ApplyTo(dataEntry, ModelState);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = $"Successfully updated market cap for stock {id}" });
            }
        }

        [HttpPatch("stocks/data/update/{ticker}/publicsentiment")]
        public async Task<IActionResult> UpdatePublicSentiment(string ticker, [FromBody] List<KeyValuePair<string, JsonPatchDocument<StockData>>> stockDataPatches)
        {
            Console.WriteLine(ticker);

            if (stockDataPatches == null)
            {
                Console.WriteLine("a");
                return BadRequest(new { message = "Patch data not found" });
            }

            List<StockData> stockData = await _context
                .StockDataEntries
                .Where(data => data.Ticker == ticker)
                .ToListAsync();

            foreach (KeyValuePair<string, JsonPatchDocument<StockData>> kvp in stockDataPatches)
            {
                string id = kvp.Key;
                JsonPatchDocument<StockData> patch = kvp.Value;

                string dateString = id.Split("_")[1];
                DateTime date = DateTime.ParseExact(dateString, "yyyyMMdd", null);
                

                StockData? data = stockData.FirstOrDefault(data => data.Ticker == ticker && data.Date == date);

                if (data != null)
                {
                    patch.ApplyTo(data, ModelState);
                }

                Console.WriteLine("apply");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _context.SaveChangesAsync();

            Console.WriteLine("save");

            return Ok();
        }
    }
}
