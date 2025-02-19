using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class StockData
{
    [Key]
    public string Id { get; set; } // Unique Identifier (Ticker + Date)

    [Required]
    public string Ticker { get; set; } // Stock Identifier

    [Required]
    public DateTime Date { get; set; } // Unique for each stock

    public decimal MarketCap { get; set; }
    public decimal ClosingPrice { get; set; }
    public double PublicSentiment { get; set; } // Range -1 to 1
    public double NewsSentiment { get; set; } // Range -1 to 1
    public decimal InterestRate { get; set; }
    public decimal ExchangeRate { get; set; }

    [Key, Column(Order = 1)]
    [ForeignKey("Ticker")]
    public Stock Stock { get; set; }

    // Constructor to auto-generate Id
    public StockData()
    {
        Id = $"{Ticker}_{Date:yyyyMMdd}";
    }
}