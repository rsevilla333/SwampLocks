using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SwampLocksDb.Models
{
    // Helper class to get stock with change
    public class StockWithChangeDto
    {
        public string Symbol { get; set; }
        public decimal MarketCap { get; set; }
        public decimal Change { get; set; }
    }

}