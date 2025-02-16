using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


public class Sector
{
    [Key]
    [Required]
    [StringLength(100)]
    public string Name { get; set; } // Unique Identifier for the Sector

    public List<Stock> Stocks { get; set; } = new List<Stock>(); // List of Stocks in the Sector
}
