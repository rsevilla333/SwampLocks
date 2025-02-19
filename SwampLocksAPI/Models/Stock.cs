using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Stock
{
    [Key]
    [Required]
    [StringLength(10)]
    public string Ticker { get; set; } // Unique Identifier for the Stock

    [Required]
    [ForeignKey("Sector")]
    public string SectorName { get; set; }  // Reference to the Sector it belongs to

    public Sector Sector { get; set; }

    public List<StockData> DataEntries { get; set; } = new List<StockData>(); // List of StockData (Per Date)
}
