using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SwampLocksDb.Models
{
    public class StockWithChangeDto
    {
        public string Symbol { get; set; }
        public decimal MarketCap { get; set; }
        public decimal Change { get; set; }
    }

}