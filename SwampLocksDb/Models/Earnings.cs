using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwampLocksDb.Models
{
    // Quaterly
    public class StockEarningStatement
    {
        [Key, Column(Order = 0)]
        [Required]
        [StringLength(10)]
        public string Ticker { get; set; } // Stock Ticker

        [Key, Column(Order = 1)]
        [Required]
        public DateTime FiscalDateEnding { get; set; } // Fiscal Date Ending

        public DateTime ReportedDate { get; set; } 
        
        public decimal ReportedEPS { get; set; }
        
        public decimal estimatedEPS { get; set; }
        
        public decimal Surprise { get; set; }
        
        public decimal SuprisePercentage { get; set; }
        
        public string ReportTime { get; set; }
        
        [ForeignKey("Ticker")]
        public virtual Stock Stock { get; set; } // Relationship with Stock entity
    }
}