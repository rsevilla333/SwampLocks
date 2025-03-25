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


    }
}
