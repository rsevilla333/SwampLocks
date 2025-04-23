using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SwampLocksDb.Models
{
    // Stock Split object/table
    public class StockSplit
    {
        [Key]
        public string Id => $"{Ticker}_{EffectiveDate:yyyyMMdd}"; // Unique Identifier (Ticker + Effective Date)

        [Required]
        public string Ticker { get; set; } // Stock Symbol

        [Required]
        public DateTime EffectiveDate { get; set; } // Date of Split

        [Required]
        [Column(TypeName = "decimal(10,4)")]
        public decimal SplitFactor { get; set; } // Split Ratio

        [ForeignKey("Ticker")]
        public virtual Stock Stock { get; set; }
    }
}