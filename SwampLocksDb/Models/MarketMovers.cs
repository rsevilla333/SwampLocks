using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace SwampLocksDb.Models
{
    // Market Mover object/table
    public class MarketMovers
    {
        public string? Ticker { get; set; }
        public string? Name { get; set; }
        public decimal? Change { get; set; }
        public decimal? ChangePercent { get; set; }
        public decimal? Price { get; set; }
        public long? Volume { get; set; }
    }
}