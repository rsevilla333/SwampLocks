using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace SwampLocksDb.Models
{ 
    // Sector object/table
    public class Sector
    {
        [Key]
        [Required]
        [StringLength(100)]
        public string Name { get; set; } // Unique Identifier for the Sector

        public List<Stock> Stocks { get; set; } = new List<Stock>(); // List of Stocks in the Sector

        public List<SectorPerformance> Performances { get; set; } = new List<SectorPerformance>(); // List of performance records based on date
        
        public List<SectorSentiment> Sentiments { get; set; } = new List<SectorSentiment>();
    }
}