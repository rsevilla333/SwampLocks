using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwampLocksAPI.Data;

namespace SwampLocksAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalController : ControllerBase
    {
        private readonly LocalContext _context;

        public LocalController(LocalContext context) 
        {
            _context = context;
        }

        [HttpGet("stocks")]
        public async Task<ActionResult<IEnumerable<Stock>>> GetStocks()
        {
            List<Stock> stocks = await _context.Stocks.ToListAsync();
            Console.WriteLine($"Retrieved {stocks.Count} stocks");
            return Ok(stocks);
        }
    }
}
