using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwampLocksDb.Models
{
public class StockData
{
    [Key]
    public string Id => $"{Ticker}_{Date:yyyyMMdd}"; // Unique Identifier (Ticker + Date)

    [Required]
    public string Ticker { get; set; } // Stock Identifier

    [Required]
    public DateTime Date { get; set; } // Unique for each stock

    public decimal MarketCap { get; set; }
    public decimal ClosingPrice { get; set; }
    public double PublicSentiment { get; set; } // Range -1 to 1
    
	[ForeignKey("Ticker")]
    public virtual Stock Stock { get; set; } 
}
}