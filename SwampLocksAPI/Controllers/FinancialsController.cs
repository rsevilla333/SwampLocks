using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public async Task<ActionResult<Stock>> Get()
        {
            Stock stock = new();
            stock.Ticker = "AMZ";
            return Ok(stock);
        }
    }
}
