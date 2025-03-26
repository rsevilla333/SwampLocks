using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwampLocksDb.Data;
using SwampLocksDb.Models;

namespace SwampLocksAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinancialsController : ControllerBase
    {
        private readonly FinancialContext _context;

        public FinancialsController(FinancialContext context) 
        {
            _context = context;
        }

        [HttpGet("ping")]

        public async Task<ActionResult<string>> PingTest()
        {
            string? connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            Stock stock = new();
            stock.Ticker = "AAPL";
            return Ok(connectionString);
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

            if (articles.Count == 0)
            {
                return NotFound();
            }

            return Ok(articles);
        }

        [HttpPatch("stocks/data/{id}")]
        public async Task<IActionResult> UpdateMarketCap(string id, [FromBody] JsonPatchDocument<StockData> stockDataPatch)
        {
            if (stockDataPatch == null)
            {
                return BadRequest(new {message = "Patch data not found"});
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
    }
}
